using IdleTerraria.Api.Data;
using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Options;
using IdleTerraria.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace IdleTerraria.IntegrationTests;

public sealed class HuntingServiceConcurrencyTests
{
    private const int BaseCycleSeconds = 3_600;
    private const int BaseExpPerCycle = 100;

    [Fact]
    public async Task Concurrent_claims_should_pay_the_same_elapsed_time_only_once()
    {
        var rootConnectionString =
            Environment.GetEnvironmentVariable(
                "IDLE_TERRARIA_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(rootConnectionString))
        {
            throw new InvalidOperationException(
                "Brak zmiennej środowiskowej " +
                "IDLE_TERRARIA_TEST_CONNECTION_STRING.");
        }

        var databaseName =
            $"idle_terraria_concurrency_{Guid.NewGuid():N}";

        var adminConnectionString =
            new NpgsqlConnectionStringBuilder(rootConnectionString)
            {
                Database = "postgres"
            }.ConnectionString;

        var testConnectionString =
            new NpgsqlConnectionStringBuilder(rootConnectionString)
            {
                Database = databaseName
            }.ConnectionString;

        var playerId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddSeconds(-3_700);

        try
        {
            await CreateDatabaseAsync(
                adminConnectionString,
                databaseName);

            await using (var setupContext =
                CreateContext(testConnectionString))
            {
                await setupContext.Database.MigrateAsync();

                await SeedWorldDataAsync(setupContext);

                var account = new Account
                {
                    Id = accountId,
                    Email = $"{Guid.NewGuid():N}@example.test",
                    PasswordHash = "test-password-hash",
                    CreatedAt = DateTime.UtcNow
                };

                var player = new Player
                {
                    Id = playerId,
                    AccountId = accountId,
                    Account = account,
                    Username = $"test_{Guid.NewGuid():N}"[..20],
                    Level = 1,
                    Experience = 0,
                    Gold = 0,
                    Stardust = 0,
                    Energy = 100,
                    ArenaElo = 1200,
                    StatsBoughtN = 0,
                    SkillPoints = 0
                };

                var activity = new PlayerActivityState
                {
                    PlayerId = playerId,
                    ActivityType = "Hunting",
                    BiomeId = 1,
                    StartedAt = startedAt,
                    LastBatchCalculatedAt = startedAt
                };

                setupContext.Accounts.Add(account);
                setupContext.Players.Add(player);
                setupContext.PlayerActivityStates.Add(activity);

                await setupContext.SaveChangesAsync();
            }

            var rules = Options.Create(new GameRulesOptions
            {
                MaxIdleHours = 24,
                BaseHuntingCycleSeconds = BaseCycleSeconds,
                BaseExpPerCycle = BaseExpPerCycle,
                BaseExpRequirementForLevelUp = 10_000,
                ExpRequirementMultiplier = 2
            });

            var firstTask = ClaimWithSeparateContextAsync(
                testConnectionString,
                playerId,
                rules);

            var secondTask = ClaimWithSeparateContextAsync(
                testConnectionString,
                playerId,
                rules);

            var results = await Task.WhenAll(
                firstTask,
                secondTask);

            Assert.Equal(
                BaseExpPerCycle,
                results.Sum(result => result.ExperienceGained));

            Assert.Contains(
                results,
                result => result.ExperienceGained == BaseExpPerCycle);

            Assert.Contains(
                results,
                result => result.ExperienceGained == 0);

            await using var verificationContext =
                CreateContext(testConnectionString);

            var savedPlayer = await verificationContext.Players
                .SingleAsync(player => player.Id == playerId);

            var savedActivity = await verificationContext.PlayerActivityStates
                .SingleAsync(activity => activity.PlayerId == playerId);

            var elapsedFromStart = savedActivity.LastBatchCalculatedAt - startedAt;

            Assert.InRange(
                elapsedFromStart.TotalSeconds,
                BaseCycleSeconds - 0.001,
                BaseCycleSeconds + 0.001);

            Assert.Equal(BaseExpPerCycle, savedPlayer.Experience);

            Assert.Equal(
                startedAt.AddSeconds(BaseCycleSeconds),
                savedActivity.LastBatchCalculatedAt,
                TimeSpan.FromMilliseconds(1));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();

            await DropDatabaseAsync(
                adminConnectionString,
                databaseName);
        }
    }

    private static async Task CreateDatabaseAsync(
        string adminConnectionString,
        string databaseName)
    {
        await using var connection =
            new NpgsqlConnection(adminConnectionString);

        await connection.OpenAsync();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            $"CREATE DATABASE \"{databaseName}\"";

        await command.ExecuteNonQueryAsync();
    }

    private static async Task DropDatabaseAsync(
        string adminConnectionString,
        string databaseName)
    {
        await using var connection =
            new NpgsqlConnection(adminConnectionString);

        await connection.OpenAsync();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)";

        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedWorldDataAsync(ApplicationDbContext context)
    {
        var category = new ItemCategory
        {
            Id = 1,
            Code = "MATERIAL",
            Name = "Material",
            Equipable = false
        };

        var itemTemplate = new ItemTemplate
        {
            Id = 1,
            CategoryId = category.Id,
            Category = category,
            Code = "ITEM_GEL",
            Name = "Gel",
            Tier = 1,
            BaseValue = 10,
            MaxStackSize = 999
        };

        var biome = new Biome
        {
            Id = 1,
            Code = "BIOME_SURFACE",
            Name = "Surface",
            MinimumLevel = 0,
            MaximumLevel = 10
        };

        var mobTemplate = new MobTemplate
        {
            Id = 1,
            Code = "MOB_GREEN_SLIME",
            Name = "Green Slime",
            Level = 1,
            BaseExperience = BaseExpPerCycle,
            MinimumGold = 1,
            MaximumGold = 5
        };

        var biomeMob = new BiomeMob
        {
            BiomeId = biome.Id,
            MobTemplateId = mobTemplate.Id,
            SpawnWeight = 100,
            Biome = biome,
            MobTemplate = mobTemplate
        };

        var lootDrop = new MobLootDrop
        {
            MobTemplateId = mobTemplate.Id,
            ItemTemplateId = itemTemplate.Id,
            MobTemplate = mobTemplate,
            ItemTemplate = itemTemplate,
            DropChance = 1.0m,
            MinimumQuantity = 1,
            MaximumQuantity = 2
        };

        context.ItemCategories.Add(category);
        context.ItemTemplates.Add(itemTemplate);
        context.Biomes.Add(biome);
        context.MobTemplates.Add(mobTemplate);
        context.BiomeMobs.Add(biomeMob);
        context.MobLootDrops.Add(lootDrop);

        await context.SaveChangesAsync();
    }

    private static async Task<HuntingClaimResult> ClaimWithSeparateContextAsync(
        string connectionString,
        Guid playerId,
        IOptions<GameRulesOptions> rules)
    {
        await using var context = CreateContext(connectionString);

        var progressionService =
            new TestPlayerProgressionService();

        var randomSource = new GameRandomSource();
        var rewardCalculator = new HuntingRewardCalculator(randomSource);

        var service = new HuntingService(
            context,
            progressionService,
            rewardCalculator,
            rules,
            NullLogger<HuntingService>.Instance,
            TimeProvider.System);

        var response = await service.ClaimRewardsAsync(playerId);

        return new HuntingClaimResult(
            response.ExperienceGained,
            response.GoldGained,
            response.DefeatedEnemies,
            response.Loot,
            response.CurrentStatus.LastClaimedAt);
    }

    private static ApplicationDbContext CreateContext(
        string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .EnableDetailedErrors()
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record HuntingClaimResult(
        int ExperienceGained,
        long GoldGained,
        int DefeatedEnemies,
        IReadOnlyCollection<HuntingLootResponse> Loot,
        DateTime? LastClaimedAt);

    private sealed class TestPlayerProgressionService
        : IPlayerProgressionService
    {
        public Task AddExperienceAsync(
            Player player,
            int expAmount,
            CancellationToken cancellationToken = default)
        {
            player.Experience += expAmount;
            return Task.CompletedTask;
        }

        public int CalculateRequiredExpForLevel(int level)
        {
            return 10_000;
        }
    }
}