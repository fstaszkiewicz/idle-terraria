using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using IdleTerraria.Api.Data;
using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Options;

namespace IdleTerraria.Api.Services
{
    public class HuntingService : IHuntingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlayerProgressionService _progressionService;
        private readonly GameRulesOptions _gameRules;
        private readonly ILogger<HuntingService> _logger;

        private const string HuntingActivityName = "Hunting";

        public HuntingService(
            ApplicationDbContext context,
            IPlayerProgressionService progressionService,
            IOptions<GameRulesOptions> gameRulesOptions,
            ILogger<HuntingService> logger)
        {
            _context = context;
            _progressionService = progressionService;
            _gameRules = gameRulesOptions.Value;
            _logger = logger;
        }

        public async Task<HuntingStatusResponse> GetStatusAsync(Guid playerId)
        {
            var activity = await _context.PlayerActivityStates
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PlayerId == playerId);

            if (activity == null || activity.ActivityType != HuntingActivityName)
            {
                return new HuntingStatusResponse { IsHunting = false };
            }

            var lastCheckpoint = activity.LastBatchCalculatedAt == default(DateTime) ? activity.StartedAt : activity.LastBatchCalculatedAt;

            return new HuntingStatusResponse
            {
                IsHunting = true,
                ZoneId = activity.BiomeId,
                StartedAt = activity.StartedAt,
                LastClaimedAt = lastCheckpoint,
                SecondsSinceLastClaim = Math.Max(0, (DateTime.UtcNow - lastCheckpoint).TotalSeconds)
            };
        }

        public async Task<HuntingStatusResponse> StartHuntingAsync(Guid playerId, StartHuntingRequest request)
        {
            var activity = await _context.PlayerActivityStates
                .FirstOrDefaultAsync(a => a.PlayerId == playerId);

            var now = DateTime.UtcNow;

            if (activity == null)
            {
                activity = new PlayerActivityState
                {
                    PlayerId = playerId,
                    ActivityType = HuntingActivityName,
                    BiomeId = request.ZoneId,
                    StartedAt = now,
                    LastBatchCalculatedAt = now
                };
                await _context.PlayerActivityStates.AddAsync(activity);
            }
            else
            {
                activity.ActivityType = HuntingActivityName;
                activity.BiomeId = request.ZoneId;
                activity.StartedAt = now;
                activity.LastBatchCalculatedAt = now;
            }

            await _context.SaveChangesAsync();

            return new HuntingStatusResponse
            {
                IsHunting = true,
                ZoneId = activity.BiomeId,
                StartedAt = activity.StartedAt,
                LastClaimedAt = activity.LastBatchCalculatedAt,
                SecondsSinceLastClaim = 0
            };
        }

        public async Task<HuntingClaimResponse> ClaimRewardsAsync(Guid playerId)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null)
                throw new KeyNotFoundException($"Gracz o ID {playerId} nie istnieje.");

            var activity = await _context.PlayerActivityStates
                .FirstOrDefaultAsync(a => a.PlayerId == playerId);

            if (activity == null || activity.ActivityType != HuntingActivityName)
                throw new InvalidOperationException("Gracz aktualnie nie prowadzi polowania.");

            var now = DateTime.UtcNow;
            var lastCheckpoint = activity.LastBatchCalculatedAt == default(DateTime) ? activity.StartedAt : activity.LastBatchCalculatedAt;

            var totalElapsed = now - lastCheckpoint;
            var maxAllowedElapsed = TimeSpan.FromHours(_gameRules.MaxIdleHours);

            if (totalElapsed > maxAllowedElapsed)
            {
                totalElapsed = maxAllowedElapsed;
            }

            int completedCycles = (int)(totalElapsed.TotalSeconds / _gameRules.BaseHuntingCycleSeconds);
            if (completedCycles <= 0)
            {
                return new HuntingClaimResponse
                {
                    ExperienceGained = 0,
                    CurrentStatus = await GetStatusAsync(playerId)
                };
            }

            int totalExpGained = completedCycles * _gameRules.BaseExpPerCycle;

            await _progressionService.AddExperienceAsync(player, totalExpGained);

            var timeConsumed = TimeSpan.FromSeconds(completedCycles * _gameRules.BaseHuntingCycleSeconds);
            activity.LastBatchCalculatedAt = lastCheckpoint + timeConsumed;

            await _context.SaveChangesAsync();

            return new HuntingClaimResponse
            {
                ExperienceGained = totalExpGained,
                CurrentStatus = new HuntingStatusResponse
                {
                    IsHunting = true,
                    ZoneId = activity.BiomeId,
                    StartedAt = activity.StartedAt,
                    LastClaimedAt = activity.LastBatchCalculatedAt,
                    SecondsSinceLastClaim = Math.Max(0, (now - activity.LastBatchCalculatedAt).TotalSeconds)
                }
            };
        }

        public async Task<HuntingClaimResponse> StopHuntingAsync(Guid playerId)
        {
            var claimResult = await ClaimRewardsAsync(playerId);

            var activity = await _context.PlayerActivityStates
                .FirstOrDefaultAsync(a => a.PlayerId == playerId);

            if (activity != null)
            {
                activity.ActivityType = "Idle";
                activity.BiomeId = null;
                activity.StartedAt = DateTime.UtcNow;
                activity.LastBatchCalculatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            claimResult.CurrentStatus = new HuntingStatusResponse { IsHunting = false };
            return claimResult;
        }
    }
}