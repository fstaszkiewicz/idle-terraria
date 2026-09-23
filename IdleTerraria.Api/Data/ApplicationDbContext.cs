using IdleTerraria.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Konta
        public DbSet<Account> Accounts { get; set; }

        // Aktywności i Profesje Gracza
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<PlayerActivityState> PlayerActivityStates { get; set; }
        public DbSet<PlayerProfession> PlayerProfessions { get; set; }

        // Przedmioty, Ekwipunek i Zestawy 
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<ItemTemplate> ItemTemplates { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Loadout> Loadouts { get; set; }

        // Drzewka Umiejętności
        public DbSet<SkillTree> SkillTrees { get; set; }
        public DbSet<PlayerUnlockedNode> PlayerUnlockedNodes { get; set; }

        // System Osad i Gildii
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<SettlementMember> SettlementMembers { get; set; }
        public DbSet<SettlementUpgrade> SettlementUpgrades { get; set; }

        // Siedziba, Sklep i Walka 
        public DbSet<HeadquarterNpc> HeadquarterNpcs { get; set; }
        public DbSet<WanderingShopStock> WanderingShopStocks { get; set; }
        public DbSet<PvpLog> PvpLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Player)
                .WithOne(p => p.Account)
                .HasForeignKey<Player>(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<PvpLog>()
                .HasOne(p => p.Attacker)
                .WithMany()
                .HasForeignKey(p => p.AttackerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PvpLog>()
                .HasOne(p => p.Defender)
                .WithMany()
                .HasForeignKey(p => p.DefenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loadout>()
                .HasOne(l => l.Weapon)
                .WithMany()
                .HasForeignKey(l => l.WeaponInvId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loadout>()
                .HasOne(l => l.Armor)
                .WithMany()
                .HasForeignKey(l => l.ArmorInvId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loadout>()
                .HasOne(l => l.Pet)
                .WithMany()
                .HasForeignKey(l => l.PetInvId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}