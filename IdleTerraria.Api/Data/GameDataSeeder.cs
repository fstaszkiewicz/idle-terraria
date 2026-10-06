using IdleTerraria.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Data;

public sealed class GameDataSeeder : IGameDataSeeder
{
    private const long SeedLockKey = 7_104_202_610_061_011_24L;

    private readonly ApplicationDbContext _context;
    private readonly ILogger<GameDataSeeder> _logger;

    public GameDataSeeder(
        ApplicationDbContext context,
        ILogger<GameDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(
    CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({SeedLockKey})",
            cancellationToken);

        await SeedCategoriesAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await SeedItemsAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await SeedCombatProfilesAsync(cancellationToken);
        await SeedBiomesAsync(cancellationToken);
        await SeedMobsAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await SeedBiomeMobRelationsAsync(cancellationToken);
        await SeedLootDropsAsync(cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Dane początkowe gry zostały poprawnie zainicjalizowane.");
    }

    private async Task SeedCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            new ItemCategoryDefinition(
                "WEAPON",
                "Weapon",
                true),

            new ItemCategoryDefinition(
                "ARMOR",
                "Armor",
                true),

            new ItemCategoryDefinition(
                "ACCESSORY",
                "Accessory",
                true),

            new ItemCategoryDefinition(
                "TOOL",
                "Tool",
                true),

            new ItemCategoryDefinition(
                "CONSUMABLE",
                "Consumable",
                false),

            new ItemCategoryDefinition(
                "MATERIAL",
                "Material",
                false),

            new ItemCategoryDefinition(
                "RESOURCE",
                "Resource",
                false),

            new ItemCategoryDefinition(
                "QUEST",
                "Quest Item",
                false)
        };

        var codes = definitions
            .Select(definition => definition.Code)
            .ToArray();

        var categories = await _context.ItemCategories
            .Where(category => codes.Contains(category.Code))
            .ToDictionaryAsync(
                category => category.Code,
                cancellationToken);

        foreach (var definition in definitions)
        {
            if (!categories.TryGetValue(
                    definition.Code,
                    out var category))
            {
                category = new ItemCategory
                {
                    Code = definition.Code
                };

                _context.ItemCategories.Add(category);
                categories.Add(definition.Code, category);
            }

            category.Name = definition.Name;
            category.Equipable = definition.Equipable;
        }
    }

    private async Task SeedItemsAsync(
        CancellationToken cancellationToken)
    {
        var categories = await _context.ItemCategories
            .ToDictionaryAsync(
                category => category.Code,
                cancellationToken);

        var definitions = new[]
        {
            new ItemDefinition(
                "WOODEN_SWORD",
                "Wooden Sword",
                "WEAPON",
                1,
                20,
                1,
                true),

            new ItemDefinition(
                "WOODEN_BOW",
                "Wooden Bow",
                "WEAPON",
                1,
                25,
                1,
                true),

            new ItemDefinition(
                "COPPER_PICKAXE",
                "Copper Pickaxe",
                "TOOL",
                1,
                35,
                1,
                true),

            new ItemDefinition(
                "COPPER_HELMET",
                "Copper Helmet",
                "ARMOR",
                1,
                30,
                1,
                true),

            new ItemDefinition(
                "SLIME_GEL",
                "Gel",
                "MATERIAL",
                1,
                2,
                999,
                true),

            new ItemDefinition(
                "WOOD",
                "Wood",
                "RESOURCE",
                1,
                3,
                999,
                true),

            new ItemDefinition(
                "LENS",
                "Lens",
                "MATERIAL",
                1,
                5,
                999,
                true),

            new ItemDefinition(
                "MUSHROOM",
                "Mushroom",
                "RESOURCE",
                1,
                4,
                999,
                true),

            new ItemDefinition(
                "HEALING_POTION",
                "Healing Potion",
                "CONSUMABLE",
                1,
                10,
                30,
                true),

            new ItemDefinition(
                "GUIDE_VOODOO_DOLL",
                "Guide Voodoo Doll",
                "QUEST",
                2,
                100,
                1,
                false)
        };

        var codes = definitions
            .Select(definition => definition.Code)
            .ToArray();

        var items = await _context.ItemTemplates
            .Where(item => codes.Contains(item.Code))
            .ToDictionaryAsync(
                item => item.Code,
                cancellationToken);

        foreach (var definition in definitions)
        {
            if (!categories.TryGetValue(
                    definition.CategoryCode,
                    out var category))
            {
                throw new InvalidOperationException(
                    $"Brak kategorii {definition.CategoryCode}.");
            }

            if (!items.TryGetValue(
                    definition.Code,
                    out var item))
            {
                item = new ItemTemplate
                {
                    Code = definition.Code
                };

                _context.ItemTemplates.Add(item);
                items.Add(definition.Code, item);
            }

            item.Name = definition.Name;
            item.Category = category;
            item.CategoryId = category.Id;
            item.Tier = definition.Tier;
            item.BaseValue = definition.BaseValue;
            item.MaxStackSize = definition.MaxStackSize;
            item.IsTradable = definition.IsTradable;
        }
    }

    private async Task SeedCombatProfilesAsync(
        CancellationToken cancellationToken)
    {
        var items = await _context.ItemTemplates
            .Where(item =>
                item.Code == "WOODEN_SWORD" ||
                item.Code == "WOODEN_BOW")
            .ToDictionaryAsync(
                item => item.Code,
                cancellationToken);

        await UpsertCombatProfileAsync(
            items,
            "WOODEN_SWORD",
            baseDamage: 7,
            attackIntervalSeconds: 1.25m,
            criticalChance: 0.05m,
            armorPenetration: 0,
            cancellationToken);

        await UpsertCombatProfileAsync(
            items,
            "WOODEN_BOW",
            baseDamage: 5,
            attackIntervalSeconds: 1.50m,
            criticalChance: 0.08m,
            armorPenetration: 0,
            cancellationToken);
    }

    private async Task UpsertCombatProfileAsync(
        IReadOnlyDictionary<string, ItemTemplate> items,
        string itemCode,
        int baseDamage,
        decimal attackIntervalSeconds,
        decimal criticalChance,
        int armorPenetration,
        CancellationToken cancellationToken)
    {
        if (!items.TryGetValue(itemCode, out var item))
        {
            throw new InvalidOperationException(
                $"Nie znaleziono przedmiotu {itemCode}.");
        }

        var profile = await _context.ItemCombatProfiles
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.ItemTemplateId == item.Id,
                cancellationToken);

        if (profile is null)
        {
            profile = new ItemCombatProfile
            {
                ItemTemplate = item,
                ItemTemplateId = item.Id
            };

            _context.ItemCombatProfiles.Add(profile);
        }

        profile.BaseDamage = baseDamage;
        profile.AttackIntervalSeconds = attackIntervalSeconds;
        profile.CriticalChance = criticalChance;
        profile.ArmorPenetration = armorPenetration;
    }

    private async Task SeedBiomesAsync(
        CancellationToken cancellationToken)
    {
        var biome = await _context.Biomes
            .SingleOrDefaultAsync(
                candidate => candidate.Code == "FOREST",
                cancellationToken);

        if (biome is null)
        {
            biome = new Biome
            {
                Code = "FOREST"
            };

            _context.Biomes.Add(biome);
        }

        biome.Name = "Forest";
        biome.MinimumLevel = 1;
        biome.MaximumLevel = 50;
        biome.IsActive = true;
    }

    private async Task SeedMobsAsync(
        CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            new MobDefinition(
                "GREEN_SLIME",
                "Green Slime",
                1,
                15,
                1,
                3),

            new MobDefinition(
                "ZOMBIE",
                "Zombie",
                2,
                25,
                2,
                6),

            new MobDefinition(
                "DEMON_EYE",
                "Demon Eye",
                3,
                40,
                4,
                10)
        };

        var codes = definitions
            .Select(definition => definition.Code)
            .ToArray();

        var mobs = await _context.MobTemplates
            .Where(mob => codes.Contains(mob.Code))
            .ToDictionaryAsync(
                mob => mob.Code,
                cancellationToken);

        foreach (var definition in definitions)
        {
            if (!mobs.TryGetValue(
                    definition.Code,
                    out var mob))
            {
                mob = new MobTemplate
                {
                    Code = definition.Code
                };

                _context.MobTemplates.Add(mob);
                mobs.Add(definition.Code, mob);
            }

            mob.Name = definition.Name;
            mob.Level = definition.Level;
            mob.BaseExperience = definition.BaseExperience;
            mob.MinimumGold = definition.MinimumGold;
            mob.MaximumGold = definition.MaximumGold;
        }
    }

    private async Task SeedBiomeMobRelationsAsync(
        CancellationToken cancellationToken)
    {
        var forest = await _context.Biomes
            .SingleAsync(
                biome => biome.Code == "FOREST",
                cancellationToken);

        var mobs = await _context.MobTemplates
            .Where(mob =>
                mob.Code == "GREEN_SLIME" ||
                mob.Code == "ZOMBIE" ||
                mob.Code == "DEMON_EYE")
            .ToDictionaryAsync(
                mob => mob.Code,
                cancellationToken);

        var relations = new[]
        {
            new BiomeMobDefinition("GREEN_SLIME", 75),
            new BiomeMobDefinition("ZOMBIE", 20),
            new BiomeMobDefinition("DEMON_EYE", 5)
        };

        foreach (var relation in relations)
        {
            if (!mobs.TryGetValue(
                    relation.MobCode,
                    out var mob))
            {
                throw new InvalidOperationException(
                    $"Nie znaleziono moba {relation.MobCode}.");
            }

            var biomeMob = await _context.BiomeMobs
                .SingleOrDefaultAsync(
                    candidate =>
                        candidate.BiomeId == forest.Id &&
                        candidate.MobTemplateId == mob.Id,
                    cancellationToken);

            if (biomeMob is null)
            {
                biomeMob = new BiomeMob
                {
                    Biome = forest,
                    MobTemplate = mob,
                    BiomeId = forest.Id,
                    MobTemplateId = mob.Id
                };

                _context.BiomeMobs.Add(biomeMob);
            }

            biomeMob.SpawnWeight = relation.SpawnWeight;
        }
    }

    private async Task SeedLootDropsAsync(
        CancellationToken cancellationToken)
    {
        var mobs = await _context.MobTemplates
            .Where(mob =>
                mob.Code == "GREEN_SLIME" ||
                mob.Code == "ZOMBIE" ||
                mob.Code == "DEMON_EYE")
            .ToDictionaryAsync(
                mob => mob.Code,
                cancellationToken);

        var items = await _context.ItemTemplates
            .Where(item =>
                item.Code == "SLIME_GEL" ||
                item.Code == "MUSHROOM" ||
                item.Code == "LENS" ||
                item.Code == "WOOD" ||
                item.Code == "HEALING_POTION")
            .ToDictionaryAsync(
                item => item.Code,
                cancellationToken);

        var definitions = new[]
        {
            new LootDefinition(
                "GREEN_SLIME",
                "SLIME_GEL",
                1.0000m,
                1,
                3),

            new LootDefinition(
                "GREEN_SLIME",
                "MUSHROOM",
                0.0500m,
                1,
                1),

            new LootDefinition(
                "ZOMBIE",
                "LENS",
                0.1500m,
                1,
                1),

            new LootDefinition(
                "ZOMBIE",
                "WOOD",
                0.2500m,
                1,
                3),

            new LootDefinition(
                "DEMON_EYE",
                "LENS",
                0.5000m,
                1,
                1),

            new LootDefinition(
                "DEMON_EYE",
                "HEALING_POTION",
                0.0500m,
                1,
                1)
        };

        foreach (var definition in definitions)
        {
            if (!mobs.TryGetValue(
                    definition.MobCode,
                    out var mob))
            {
                throw new InvalidOperationException(
                    $"Nie znaleziono moba {definition.MobCode}.");
            }

            if (!items.TryGetValue(
                    definition.ItemCode,
                    out var item))
            {
                throw new InvalidOperationException(
                    $"Nie znaleziono przedmiotu {definition.ItemCode}.");
            }

            var drop = await _context.MobLootDrops
                .SingleOrDefaultAsync(
                    candidate =>
                        candidate.MobTemplateId == mob.Id &&
                        candidate.ItemTemplateId == item.Id,
                    cancellationToken);

            if (drop is null)
            {
                drop = new MobLootDrop
                {
                    MobTemplate = mob,
                    ItemTemplate = item,
                    MobTemplateId = mob.Id,
                    ItemTemplateId = item.Id
                };

                _context.MobLootDrops.Add(drop);
            }

            drop.DropChance = definition.DropChance;
            drop.MinimumQuantity = definition.MinimumQuantity;
            drop.MaximumQuantity = definition.MaximumQuantity;
        }
    }

    private sealed record ItemCategoryDefinition(
        string Code,
        string Name,
        bool Equipable);

    private sealed record ItemDefinition(
        string Code,
        string Name,
        string CategoryCode,
        int Tier,
        long BaseValue,
        int MaxStackSize,
        bool IsTradable);

    private sealed record MobDefinition(
        string Code,
        string Name,
        int Level,
        int BaseExperience,
        long MinimumGold,
        long MaximumGold);

    private sealed record BiomeMobDefinition(
        string MobCode,
        int SpawnWeight);

    private sealed record LootDefinition(
        string MobCode,
        string ItemCode,
        decimal DropChance,
        int MinimumQuantity,
        int MaximumQuantity);
}