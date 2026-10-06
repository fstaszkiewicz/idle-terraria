using IdleTerraria.Api.Data;
using IdleTerraria.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace IdleTerraria.Api.Tests;

public sealed class EntityModelTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=model_tests;Username=test;Password=test")
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void ItemCategoryMapsToLowercaseColumns()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(ItemCategory))!;
        var table = StoreObjectIdentifier.Table(
            entityType.GetTableName()!,
            entityType.GetSchema());

        Assert.Equal("item_categories", entityType.GetTableName());
        Assert.Equal("id", entityType.FindProperty(nameof(ItemCategory.Id))!.GetColumnName(table));
        Assert.Equal("code", entityType.FindProperty(nameof(ItemCategory.Code))!.GetColumnName(table));
        Assert.Equal("name", entityType.FindProperty(nameof(ItemCategory.Name))!.GetColumnName(table));
        Assert.Equal("equipable", entityType.FindProperty(nameof(ItemCategory.Equipable))!.GetColumnName(table));
        Assert.DoesNotContain(entityType.GetProperties(), property =>
            new[] { "Id", "Code", "Name", "Equipable" }.Contains(property.GetColumnName(table)));
    }

    [Fact]
    public void ItemCategoryAndItemTemplateRelationshipsAreRequiredAndRestricted()
    {
        using var context = CreateContext();

        var category = context.Model.FindEntityType(typeof(ItemCategory))!;
        var item = context.Model.FindEntityType(typeof(ItemTemplate))!;
        var categoryForeignKey = Assert.Single(item.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(ItemCategory));

        Assert.Equal(nameof(ItemTemplate.CategoryId), Assert.Single(categoryForeignKey.Properties).Name);
        Assert.False(categoryForeignKey.Properties.Single().IsNullable);
        Assert.Equal(DeleteBehavior.Restrict, categoryForeignKey.DeleteBehavior);
        Assert.Contains(category.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ItemCategory.Code)]));
        Assert.Contains(item.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ItemTemplate.Code)]));
        Assert.Contains(item.GetIndexes(), index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ItemTemplate.CategoryId), nameof(ItemTemplate.Tier)]));
    }

    [Fact]
    public void MobLootDropHasOneExplicitRestrictedItemTemplateRelationship()
    {
        using var context = CreateContext();

        var lootDrop = context.Model.FindEntityType(typeof(MobLootDrop))!;
        var relationships = lootDrop.GetForeignKeys()
            .Where(foreignKey =>
                foreignKey.PrincipalEntityType.ClrType == typeof(ItemTemplate))
            .ToArray();

        var relationship = Assert.Single(relationships);

        Assert.Equal(nameof(MobLootDrop.ItemTemplateId),
            Assert.Single(relationship.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, relationship.DeleteBehavior);
        Assert.DoesNotContain(lootDrop.GetProperties(), property => property.IsShadowProperty());
    }

    [Fact]
    public void RequiredDomainCheckConstraintsArePresent()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;

        var requiredConstraints = new Dictionary<Type, string[]>
        {
            [typeof(Biome)] =
            [
                "CK_biomes_minimum_level_non_negative",
                "CK_biomes_maximum_level_valid"
            ],
            [typeof(MobTemplate)] =
            [
                "CK_mob_templates_level_positive",
                "CK_mob_templates_base_experience_non_negative",
                "CK_mob_templates_gold_range_valid"
            ],
            [typeof(BiomeMob)] = ["CK_biome_mobs_spawn_weight_positive"],
            [typeof(MobLootDrop)] =
            [
                "CK_mob_loot_drops_drop_chance_range",
                "CK_mob_loot_drops_quantity_range"
            ],
            [typeof(ItemTemplate)] =
            [
                "CK_item_templates_tier_range",
                "CK_item_templates_base_value_non_negative",
                "CK_item_templates_max_stack_size_positive"
            ],
            [typeof(ItemCombatProfile)] =
            [
                "CK_item_combat_profiles_base_damage_non_negative",
                "CK_item_combat_profiles_attack_interval_positive",
                "CK_item_combat_profiles_critical_chance_range",
                "CK_item_combat_profiles_armor_penetration_non_negative"
            ],
            [typeof(Inventory)] =
            [
                "CK_inventory_quantity_positive",
                "CK_inventory_upgrade_level_range"
            ]
        };

        foreach (var (entityType, constraintNames) in requiredConstraints)
        {
            var metadata = model.FindEntityType(entityType)!;
            var actualNames = metadata.GetCheckConstraints()
                .Select(constraint => constraint.Name)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var constraintName in constraintNames)
            {
                Assert.Contains(constraintName, actualNames);
            }
        }
    }
}
