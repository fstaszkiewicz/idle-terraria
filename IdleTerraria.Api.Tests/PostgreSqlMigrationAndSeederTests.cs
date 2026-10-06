using IdleTerraria.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace IdleTerraria.Api.Tests;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    "IDLE_TERRARIA_TEST_CONNECTION_STRING")))
        {
            Skip = "Set IDLE_TERRARIA_TEST_CONNECTION_STRING to a PostgreSQL connection with CREATEDB permission.";
        }
    }
}

public sealed class PostgreSqlMigrationAndSeederTests
{
    [PostgreSqlFact]
    public async Task FreshDatabaseMigratesAndSeederIsIdempotent()
    {
        var rootConnectionString = Environment.GetEnvironmentVariable(
            "IDLE_TERRARIA_TEST_CONNECTION_STRING")!;
        var databaseName = $"idle_terraria_test_{Guid.NewGuid():N}";
        var adminConnectionString = new NpgsqlConnectionStringBuilder(
            rootConnectionString)
        {
            Database = "postgres"
        }.ConnectionString;
        var testConnectionString = new NpgsqlConnectionStringBuilder(
            rootConnectionString)
        {
            Database = databaseName
        }.ConnectionString;

        try
        {
            await using (var adminConnection = new NpgsqlConnection(
                             adminConnectionString))
            {
                await adminConnection.OpenAsync();
                await using var createDatabase = adminConnection.CreateCommand();
                createDatabase.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                await createDatabase.ExecuteNonQueryAsync();
            }

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(testConnectionString)
                .Options;

            await using var context = new ApplicationDbContext(options);
            await context.Database.MigrateAsync();

            var appliedMigrations = await context.Database
                .GetAppliedMigrationsAsync();
            Assert.NotEmpty(appliedMigrations);

            var categoryColumns = await ReadCategoryColumnsAsync(context);
            Assert.Equal(
                new[] { "code", "equipable", "id", "name" },
                categoryColumns.Order(StringComparer.Ordinal).ToArray());

            var seeder = new GameDataSeeder(
                context,
                NullLogger<GameDataSeeder>.Instance);

            await seeder.SeedAsync();
            var firstCounts = await ReadSeedCountsAsync(context);

            Assert.Equal((8, 10, 2, 1, 3, 3, 6), firstCounts);
            Assert.True(await context.Biomes.AnyAsync(biome =>
                biome.Code == "FOREST"));
            Assert.Equal(3, await context.MobTemplates.CountAsync(mob =>
                mob.Code == "GREEN_SLIME" ||
                mob.Code == "ZOMBIE" ||
                mob.Code == "DEMON_EYE"));

            await seeder.SeedAsync();
            var secondCounts = await ReadSeedCountsAsync(context);

            Assert.Equal(firstCounts, secondCounts);
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();

            await using var adminConnection = new NpgsqlConnection(
                adminConnectionString);
            await adminConnection.OpenAsync();
            await using var dropDatabase = adminConnection.CreateCommand();
            dropDatabase.CommandText =
                $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)";
            await dropDatabase.ExecuteNonQueryAsync();
        }
    }

    private static async Task<IReadOnlyCollection<string>> ReadCategoryColumnsAsync(
        ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT column_name
                FROM information_schema.columns
                WHERE table_schema = current_schema()
                  AND table_name = 'item_categories'
                ORDER BY column_name
                """;

            await using var reader = await command.ExecuteReaderAsync();
            var columns = new List<string>();

            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }

            return columns;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static async Task<(int Categories, int Items, int Profiles, int Biomes, int Mobs, int Relations, int Drops)>
        ReadSeedCountsAsync(ApplicationDbContext context)
    {
        return (
            await context.ItemCategories.CountAsync(),
            await context.ItemTemplates.CountAsync(),
            await context.ItemCombatProfiles.CountAsync(),
            await context.Biomes.CountAsync(),
            await context.MobTemplates.CountAsync(),
            await context.BiomeMobs.CountAsync(),
            await context.MobLootDrops.CountAsync());
    }
}
