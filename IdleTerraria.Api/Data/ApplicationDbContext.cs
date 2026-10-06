using IdleTerraria.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Konta
    public DbSet<Account> Accounts => Set<Account>();

    // Gracze i aktywności
    public DbSet<Player> Players => Set<Player>();

    public DbSet<PlayerStats> PlayerStats => Set<PlayerStats>();

    public DbSet<PlayerActivityState> PlayerActivityStates =>
        Set<PlayerActivityState>();

    public DbSet<PlayerProfession> PlayerProfessions =>
        Set<PlayerProfession>();

    // Świat, biomy i przeciwnicy
    public DbSet<Biome> Biomes => Set<Biome>();

    public DbSet<MobTemplate> MobTemplates => Set<MobTemplate>();

    public DbSet<BiomeMob> BiomeMobs => Set<BiomeMob>();

    public DbSet<MobLootDrop> MobLootDrops => Set<MobLootDrop>();

    // Przedmioty, ekwipunek i loadouty
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();

    public DbSet<ItemTemplate> ItemTemplates => Set<ItemTemplate>();

    public DbSet<ItemCombatProfile> ItemCombatProfiles =>
        Set<ItemCombatProfile>();

    public DbSet<Inventory> Inventories => Set<Inventory>();

    public DbSet<Loadout> Loadouts => Set<Loadout>();

    // Drzewka umiejętności
    public DbSet<SkillTree> SkillTrees => Set<SkillTree>();

    public DbSet<PlayerUnlockedNode> PlayerUnlockedNodes =>
        Set<PlayerUnlockedNode>();

    // Osady i gildie
    public DbSet<Settlement> Settlements => Set<Settlement>();

    public DbSet<SettlementMember> SettlementMembers =>
        Set<SettlementMember>();

    public DbSet<SettlementUpgrade> SettlementUpgrades =>
        Set<SettlementUpgrade>();

    // Siedziba, sklep i walka
    public DbSet<HeadquarterNpc> HeadquarterNpcs =>
        Set<HeadquarterNpc>();

    public DbSet<WanderingShopStock> WanderingShopStocks =>
        Set<WanderingShopStock>();

    public DbSet<PvpLog> PvpLogs => Set<PvpLog>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureExistingRelationships(modelBuilder);
        ConfigureWorldAndMobRelationships(modelBuilder);
        ConfigureItemRelationships(modelBuilder);
    }

    private static void ConfigureExistingRelationships(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasOne(account => account.Player)
            .WithOne(player => player.Account)
            .HasForeignKey<Player>(
                player => player.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Account>()
            .HasIndex(account => account.Email)
            .IsUnique();

        modelBuilder.Entity<Player>()
            .HasIndex(player => player.Username)
            .IsUnique();

        modelBuilder.Entity<PvpLog>()
            .HasOne(log => log.Attacker)
            .WithMany()
            .HasForeignKey(log => log.AttackerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PvpLog>()
            .HasOne(log => log.Defender)
            .WithMany()
            .HasForeignKey(log => log.DefenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loadout>()
            .HasOne(loadout => loadout.Weapon)
            .WithMany()
            .HasForeignKey(loadout => loadout.WeaponInvId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loadout>()
            .HasOne(loadout => loadout.Armor)
            .WithMany()
            .HasForeignKey(loadout => loadout.ArmorInvId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loadout>()
            .HasOne(loadout => loadout.Pet)
            .WithMany()
            .HasForeignKey(loadout => loadout.PetInvId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureWorldAndMobRelationships(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Biome>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_biomes_minimum_level_non_negative",
                    "\"minimum_level\" >= 0");

                table.HasCheckConstraint(
                    "CK_biomes_maximum_level_valid",
                    "\"maximum_level\" IS NULL OR " +
                    "\"maximum_level\" >= \"minimum_level\"");
            });

            entity.HasKey(biome => biome.Id);

            entity.HasIndex(biome => biome.Code)
                .IsUnique();

            entity.Property(biome => biome.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(biome => biome.Name)
                .HasMaxLength(100)
                .IsRequired();

        });

        modelBuilder.Entity<MobTemplate>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_mob_templates_level_positive",
                    "\"level\" > 0");

                table.HasCheckConstraint(
                    "CK_mob_templates_base_experience_non_negative",
                    "\"base_experience\" >= 0");

                table.HasCheckConstraint(
                    "CK_mob_templates_gold_range_valid",
                    "\"minimum_gold\" >= 0 AND " +
                    "\"maximum_gold\" >= \"minimum_gold\"");
            });

            entity.HasKey(mob => mob.Id);

            entity.HasIndex(mob => mob.Code)
                .IsUnique();

            entity.Property(mob => mob.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(mob => mob.Name)
                .HasMaxLength(100)
                .IsRequired();

        });

        modelBuilder.Entity<BiomeMob>(entity =>
        {
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_biome_mobs_spawn_weight_positive",
                    "\"spawn_weight\" > 0"));

            entity.HasKey(item => new
            {
                item.BiomeId,
                item.MobTemplateId
            });

            entity.HasIndex(item => item.MobTemplateId);

            entity.HasOne(item => item.Biome)
                .WithMany(biome => biome.MobPool)
                .HasForeignKey(item => item.BiomeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.MobTemplate)
                .WithMany(mob => mob.Biomes)
                .HasForeignKey(item => item.MobTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<MobLootDrop>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_mob_loot_drops_drop_chance_range",
                    "\"drop_chance\" >= 0 AND \"drop_chance\" <= 1");

                table.HasCheckConstraint(
                    "CK_mob_loot_drops_quantity_range",
                    "\"minimum_quantity\" > 0 AND " +
                    "\"maximum_quantity\" >= \"minimum_quantity\"");
            });

            entity.HasKey(drop => drop.Id);

            entity.HasIndex(drop => new
            {
                drop.MobTemplateId,
                drop.ItemTemplateId
            }).IsUnique();

            entity.Property(drop => drop.DropChance)
                .HasPrecision(5, 4)
                .IsRequired();

            entity.HasOne(drop => drop.MobTemplate)
                .WithMany(mob => mob.LootDrops)
                .HasForeignKey(drop => drop.MobTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(drop => drop.ItemTemplate)
                .WithMany(item => item.LootDrops)
                .HasForeignKey(drop => drop.ItemTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

        });
    }

    private static void ConfigureItemRelationships(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(category => category.Id);

            entity.HasIndex(category => category.Code)
                .IsUnique();
        });

        modelBuilder.Entity<ItemTemplate>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_item_templates_tier_range",
                    "\"tier\" >= 1 AND \"tier\" <= 12");

                table.HasCheckConstraint(
                    "CK_item_templates_base_value_non_negative",
                    "\"base_value\" >= 0");

                table.HasCheckConstraint(
                    "CK_item_templates_max_stack_size_positive",
                    "\"max_stack_size\" > 0");
            });

            entity.HasKey(item => item.Id);

            entity.HasIndex(item => item.Code)
                .IsUnique();

            entity.HasIndex(item => new
            {
                item.CategoryId,
                item.Tier
            });

            entity.Property(item => item.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(item => item.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(item => item.BaseValue)
                .HasColumnType("bigint");

            entity.HasOne(item => item.Category)
                .WithMany(category => category.ItemTemplates)
                .HasForeignKey(item => item.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<ItemCombatProfile>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_item_combat_profiles_base_damage_non_negative",
                    "\"base_damage\" >= 0");

                table.HasCheckConstraint(
                    "CK_item_combat_profiles_attack_interval_positive",
                    "\"attack_interval_seconds\" > 0");

                table.HasCheckConstraint(
                    "CK_item_combat_profiles_critical_chance_range",
                    "\"critical_chance\" >= 0 AND " +
                    "\"critical_chance\" <= 1");

                table.HasCheckConstraint(
                    "CK_item_combat_profiles_armor_penetration_non_negative",
                    "\"armor_penetration\" >= 0");
            });

            entity.HasKey(profile => profile.ItemTemplateId);

            entity.Property(profile => profile.AttackIntervalSeconds)
                .HasPrecision(10, 3)
                .IsRequired();

            entity.Property(profile => profile.CriticalChance)
                .HasPrecision(5, 4)
                .IsRequired();

            entity.HasOne(profile => profile.ItemTemplate)
                .WithOne(item => item.CombatProfile)
                .HasForeignKey<ItemCombatProfile>(
                    profile => profile.ItemTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_inventory_quantity_positive",
                    "\"quantity\" > 0");

                table.HasCheckConstraint(
                    "CK_inventory_upgrade_level_range",
                    "\"upgrade_level\" >= 0 AND " +
                    "\"upgrade_level\" <= 10");
            });

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Prefix)
                .HasMaxLength(30)
                .HasDefaultValue("Normal");

            entity.HasOne(item => item.Player)
                .WithMany()
                .HasForeignKey(item => item.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Template)
                .WithMany()
                .HasForeignKey(item => item.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

        });
    }
}