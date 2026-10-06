using System.Data;
using IdleTerraria.Api.Data;
using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IdleTerraria.Api.Services;

public sealed class HuntingService : IHuntingService
{
    private const string HuntingActivityName = "Hunting";
    private const string IdleActivityName = "Idle";
    private const string DefaultLootPrefix = "Normal";

    private readonly ApplicationDbContext _context;
    private readonly IPlayerProgressionService _progressionService;
    private readonly IHuntingRewardCalculator _rewardCalculator;
    private readonly GameRulesOptions _gameRules;
    private readonly ILogger<HuntingService> _logger;
    private readonly TimeProvider _timeProvider;

    public HuntingService(
        ApplicationDbContext context,
        IPlayerProgressionService progressionService,
        IHuntingRewardCalculator rewardCalculator,
        IOptions<GameRulesOptions> gameRulesOptions,
        ILogger<HuntingService> logger,
        TimeProvider timeProvider)
    {
        _context = context;
        _progressionService = progressionService;
        _rewardCalculator = rewardCalculator;
        _gameRules = gameRulesOptions.Value;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<HuntingStatusResponse> GetStatusAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        var activity = await _context.PlayerActivityStates
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.PlayerId == playerId,
                cancellationToken);

        if (activity is null ||
            !string.Equals(
                activity.ActivityType,
                HuntingActivityName,
                StringComparison.Ordinal))
        {
            return new HuntingStatusResponse
            {
                IsHunting = false
            };
        }

        var checkpoint = GetLastCheckpoint(activity);

        return BuildStatusResponse(
            activity,
            checkpoint,
            GetUtcNow());
    }

    public async Task<HuntingStatusResponse> StartHuntingAsync(
        Guid playerId,
        StartHuntingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var biomeExists = await _context.Biomes
            .AnyAsync(
                biome =>
                    biome.Id == request.ZoneId &&
                    biome.IsActive,
                cancellationToken);

        if (!biomeExists)
        {
            throw new KeyNotFoundException(
                $"Aktywny biom o ID {request.ZoneId} nie istnieje.");
        }

        var now = GetUtcNow();

        var activity = await _context.PlayerActivityStates
            .SingleOrDefaultAsync(
                item => item.PlayerId == playerId,
                cancellationToken);

        if (activity is null)
        {
            activity = new PlayerActivityState
            {
                PlayerId = playerId,
                ActivityType = HuntingActivityName,
                BiomeId = request.ZoneId,
                StartedAt = now,
                LastBatchCalculatedAt = now
            };

            await _context.PlayerActivityStates.AddAsync(
                activity,
                cancellationToken);
        }
        else
        {
            activity.ActivityType = HuntingActivityName;
            activity.BiomeId = request.ZoneId;
            activity.StartedAt = now;
            activity.LastBatchCalculatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return BuildStatusResponse(
            activity,
            activity.LastBatchCalculatedAt,
            now);
    }

    public async Task<HuntingClaimResponse> ClaimRewardsAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        ValidateGameRules();

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

        var player = await _context.Players
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM players
                WHERE id = {playerId}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (player is null)
        {
            throw new KeyNotFoundException(
                $"Gracz o ID {playerId} nie istnieje.");
        }

        var activity = await _context.PlayerActivityStates
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM player_activity_states
                WHERE player_id = {playerId}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (activity is null ||
            !string.Equals(
                activity.ActivityType,
                HuntingActivityName,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Gracz aktualnie nie prowadzi polowania.");
        }

        var now = GetUtcNow();
        var lastCheckpoint = GetLastCheckpoint(activity);
        var elapsed = now - lastCheckpoint;

        if (elapsed < TimeSpan.Zero)
        {
            _logger.LogWarning(
                "Timestamp aktywności gracza {PlayerId} znajduje się " +
                "w przyszłości.",
                playerId);

            elapsed = TimeSpan.Zero;
        }

        var maxAllowedElapsed =
            TimeSpan.FromHours(_gameRules.MaxIdleHours);

        if (elapsed > maxAllowedElapsed)
        {
            elapsed = maxAllowedElapsed;
        }

        var completedCycles = (long)(
            elapsed.TotalSeconds /
            _gameRules.BaseHuntingCycleSeconds);

        if (completedCycles <= 0)
        {
            await transaction.CommitAsync(cancellationToken);

            return new HuntingClaimResponse
            {
                CurrentStatus = BuildStatusResponse(
                    activity,
                    lastCheckpoint,
                    now)
            };
        }

        if (activity.BiomeId is null)
        {
            throw new InvalidOperationException(
                "Aktywność polowania nie posiada przypisanego biomu.");
        }

        var biome = await _context.Biomes
            .Include(item => item.MobPool)
                .ThenInclude(item => item.MobTemplate)
                    .ThenInclude(item => item.LootDrops)
                        .ThenInclude(item => item.ItemTemplate)
            .SingleOrDefaultAsync(
                item =>
                    item.Id == activity.BiomeId.Value &&
                    item.IsActive,
                cancellationToken);

        if (biome is null)
        {
            throw new KeyNotFoundException(
                $"Aktywny biom o ID {activity.BiomeId} nie istnieje.");
        }

        var calculation = _rewardCalculator.Calculate(
            biome.MobPool.ToArray(),
            completedCycles,
            cancellationToken);

        await _progressionService.AddExperienceAsync(
            player,
            calculation.ExperienceGained,
            cancellationToken);

        player.Gold = checked(
            player.Gold +
            calculation.GoldGained);

        await ApplyLootAsync(
            playerId,
            calculation.Loot,
            cancellationToken);

        var consumedTime = TimeSpan.FromSeconds(
            checked(
                completedCycles *
                (long)_gameRules.BaseHuntingCycleSeconds));

        activity.LastBatchCalculatedAt =
            lastCheckpoint + consumedTime;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new HuntingClaimResponse
        {
            ExperienceGained = calculation.ExperienceGained,
            GoldGained = calculation.GoldGained,
            DefeatedEnemies = calculation.DefeatedEnemies,
            Loot = calculation.Loot
                .Select(item => new HuntingLootResponse
                {
                    ItemTemplateId = item.ItemTemplateId,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Quantity = item.Quantity
                })
                .ToArray(),
            CurrentStatus = BuildStatusResponse(
                activity,
                activity.LastBatchCalculatedAt,
                now)
        };
    }

    public async Task<HuntingClaimResponse> StopHuntingAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        var claimResult = await ClaimRewardsAsync(
            playerId,
            cancellationToken);

        var activity = await _context.PlayerActivityStates
            .SingleOrDefaultAsync(
                item => item.PlayerId == playerId,
                cancellationToken);

        if (activity is not null)
        {
            var now = GetUtcNow();

            activity.ActivityType = IdleActivityName;
            activity.BiomeId = null;
            activity.StartedAt = now;
            activity.LastBatchCalculatedAt = now;

            await _context.SaveChangesAsync(cancellationToken);
        }

        return new HuntingClaimResponse
        {
            ExperienceGained = claimResult.ExperienceGained,
            GoldGained = claimResult.GoldGained,
            DefeatedEnemies = claimResult.DefeatedEnemies,
            Loot = claimResult.Loot,
            CurrentStatus = new HuntingStatusResponse
            {
                IsHunting = false
            }
        };
    }

    private async Task ApplyLootAsync(
        Guid playerId,
        IReadOnlyCollection<HuntingLootReward> loot,
        CancellationToken cancellationToken)
    {
        foreach (var reward in loot)
        {
            var remaining = reward.Quantity;

            while (remaining > 0)
            {
                var existingItem = await _context.Inventories
                    .Where(item =>
                        item.PlayerId == playerId &&
                        item.TemplateId ==
                            reward.ItemTemplateId &&
                        item.Prefix == DefaultLootPrefix &&
                        item.UpgradeLevel == 0 &&
                        item.Quantity < reward.MaxStackSize)
                    .OrderBy(item => item.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingItem is null)
                {
                    var quantity = Math.Min(
                        remaining,
                        reward.MaxStackSize);

                    _context.Inventories.Add(
                        new Inventory
                        {
                            Id = Guid.NewGuid(),
                            PlayerId = playerId,
                            TemplateId = reward.ItemTemplateId,
                            Prefix = DefaultLootPrefix,
                            UpgradeLevel = 0,
                            Quantity = quantity
                        });

                    remaining -= quantity;
                    continue;
                }

                var availableSpace =
                    reward.MaxStackSize -
                    existingItem.Quantity;

                var quantityToAdd = Math.Min(
                    remaining,
                    availableSpace);

                existingItem.Quantity = checked(
                    existingItem.Quantity +
                    quantityToAdd);

                remaining -= quantityToAdd;
            }
        }
    }

    private void ValidateGameRules()
    {
        if (_gameRules.MaxIdleHours <= 0)
        {
            throw new InvalidOperationException(
                "GameRules:MaxIdleHours musi być większe od zera.");
        }

        if (_gameRules.BaseHuntingCycleSeconds <= 0)
        {
            throw new InvalidOperationException(
                "GameRules:BaseHuntingCycleSeconds musi być większe od zera.");
        }

        if (_gameRules.BaseExpPerCycle < 0)
        {
            throw new InvalidOperationException(
                "GameRules:BaseExpPerCycle nie może być ujemne.");
        }
    }

    private static DateTime GetLastCheckpoint(
        PlayerActivityState activity)
    {
        return activity.LastBatchCalculatedAt == default
            ? activity.StartedAt
            : activity.LastBatchCalculatedAt;
    }

    private HuntingStatusResponse BuildStatusResponse(
        PlayerActivityState activity,
        DateTime lastCheckpoint,
        DateTime now)
    {
        return new HuntingStatusResponse
        {
            IsHunting = string.Equals(
                activity.ActivityType,
                HuntingActivityName,
                StringComparison.Ordinal),
            ZoneId = activity.BiomeId,
            StartedAt = activity.StartedAt,
            LastClaimedAt = lastCheckpoint,
            SecondsSinceLastClaim = Math.Max(
                0,
                (now - lastCheckpoint).TotalSeconds)
        };
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}