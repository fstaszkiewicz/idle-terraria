using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdleTerraria.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "biomes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    minimum_level = table.Column<int>(type: "integer", nullable: false),
                    maximum_level = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biomes", x => x.id);
                    table.CheckConstraint("CK_biomes_maximum_level_valid", "\"maximum_level\" IS NULL OR \"maximum_level\" >= \"minimum_level\"");
                    table.CheckConstraint("CK_biomes_minimum_level_non_negative", "\"minimum_level\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "item_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    equipable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mob_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    base_experience = table.Column<int>(type: "integer", nullable: false),
                    minimum_gold = table.Column<long>(type: "bigint", nullable: false),
                    maximum_gold = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mob_templates", x => x.id);
                    table.CheckConstraint("CK_mob_templates_base_experience_non_negative", "\"base_experience\" >= 0");
                    table.CheckConstraint("CK_mob_templates_gold_range_valid", "\"minimum_gold\" >= 0 AND \"maximum_gold\" >= \"minimum_gold\"");
                    table.CheckConstraint("CK_mob_templates_level_positive", "\"level\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "skill_trees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tree_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    node_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    stat_modifier = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    modifier_value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_trees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<long>(type: "bigint", nullable: false),
                    gold = table.Column<long>(type: "bigint", nullable: false),
                    stardust = table.Column<int>(type: "integer", nullable: false),
                    energy = table.Column<int>(type: "integer", nullable: false),
                    arena_elo = table.Column<int>(type: "integer", nullable: false),
                    current_biome_id = table.Column<int>(type: "integer", nullable: true),
                    stats_bought_n = table.Column<int>(type: "integer", nullable: false),
                    skill_points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                    table.ForeignKey(
                        name: "FK_players_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    tier = table.Column<int>(type: "integer", nullable: false),
                    base_value = table.Column<long>(type: "bigint", nullable: false),
                    max_stack_size = table.Column<int>(type: "integer", nullable: false),
                    is_tradable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_templates", x => x.id);
                    table.CheckConstraint("CK_item_templates_base_value_non_negative", "\"base_value\" >= 0");
                    table.CheckConstraint("CK_item_templates_max_stack_size_positive", "\"max_stack_size\" > 0");
                    table.CheckConstraint("CK_item_templates_tier_range", "\"tier\" >= 1 AND \"tier\" <= 12");
                    table.ForeignKey(
                        name: "FK_item_templates_item_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "item_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "biome_mobs",
                columns: table => new
                {
                    biome_id = table.Column<int>(type: "integer", nullable: false),
                    mob_template_id = table.Column<int>(type: "integer", nullable: false),
                    spawn_weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biome_mobs", x => new { x.biome_id, x.mob_template_id });
                    table.CheckConstraint("CK_biome_mobs_spawn_weight_positive", "\"spawn_weight\" > 0");
                    table.ForeignKey(
                        name: "FK_biome_mobs_biomes_biome_id",
                        column: x => x.biome_id,
                        principalTable: "biomes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_biome_mobs_mob_templates_mob_template_id",
                        column: x => x.mob_template_id,
                        principalTable: "mob_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "headquarter_npcs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<Guid>(type: "uuid", nullable: true),
                    npc_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    morale_percentage = table.Column<int>(type: "integer", nullable: false),
                    is_unlocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_headquarter_npcs", x => x.id);
                    table.ForeignKey(
                        name: "FK_headquarter_npcs_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "player_activity_states",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    biome_id = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_batch_calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_activity_states", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_player_activity_states_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_professions",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profession_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    experience = table.Column<long>(type: "bigint", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_professions", x => new { x.player_id, x.profession_type });
                    table.ForeignKey(
                        name: "FK_player_professions_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_stats",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stat_strength = table.Column<int>(type: "integer", nullable: false),
                    stat_dexterity = table.Column<int>(type: "integer", nullable: false),
                    stat_luck = table.Column<int>(type: "integer", nullable: false),
                    stat_vitality = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_stats", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_player_stats_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pvp_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attacker_id = table.Column<Guid>(type: "uuid", nullable: true),
                    defender_id = table.Column<Guid>(type: "uuid", nullable: true),
                    battle_data = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pvp_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_pvp_logs_players_attacker_id",
                        column: x => x.attacker_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pvp_logs_players_defender_id",
                        column: x => x.defender_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "settlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    leader_id = table.Column<Guid>(type: "uuid", nullable: true),
                    glory_emblems = table.Column<int>(type: "integer", nullable: false),
                    gold_vault = table.Column<long>(type: "bigint", nullable: false),
                    elo_rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlements", x => x.id);
                    table.ForeignKey(
                        name: "FK_settlements_players_leader_id",
                        column: x => x.leader_id,
                        principalTable: "players",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: true),
                    template_id = table.Column<int>(type: "integer", nullable: true),
                    prefix = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, defaultValue: "Normal"),
                    upgrade_level = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory", x => x.id);
                    table.CheckConstraint("CK_inventory_quantity_positive", "\"quantity\" > 0");
                    table.CheckConstraint("CK_inventory_upgrade_level_range", "\"upgrade_level\" >= 0 AND \"upgrade_level\" <= 10");
                    table.ForeignKey(
                        name: "FK_inventory_item_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "item_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_combat_profiles",
                columns: table => new
                {
                    item_template_id = table.Column<int>(type: "integer", nullable: false),
                    base_damage = table.Column<int>(type: "integer", nullable: false),
                    attack_interval_seconds = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    critical_chance = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    armor_penetration = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_combat_profiles", x => x.item_template_id);
                    table.CheckConstraint("CK_item_combat_profiles_armor_penetration_non_negative", "\"armor_penetration\" >= 0");
                    table.CheckConstraint("CK_item_combat_profiles_attack_interval_positive", "\"attack_interval_seconds\" > 0");
                    table.CheckConstraint("CK_item_combat_profiles_base_damage_non_negative", "\"base_damage\" >= 0");
                    table.CheckConstraint("CK_item_combat_profiles_critical_chance_range", "\"critical_chance\" >= 0 AND \"critical_chance\" <= 1");
                    table.ForeignKey(
                        name: "FK_item_combat_profiles_item_templates_item_template_id",
                        column: x => x.item_template_id,
                        principalTable: "item_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mob_loot_drops",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mob_template_id = table.Column<int>(type: "integer", nullable: false),
                    item_template_id = table.Column<int>(type: "integer", nullable: false),
                    drop_chance = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    minimum_quantity = table.Column<int>(type: "integer", nullable: false),
                    maximum_quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mob_loot_drops", x => x.id);
                    table.CheckConstraint("CK_mob_loot_drops_drop_chance_range", "\"drop_chance\" >= 0 AND \"drop_chance\" <= 1");
                    table.CheckConstraint("CK_mob_loot_drops_quantity_range", "\"minimum_quantity\" > 0 AND \"maximum_quantity\" >= \"minimum_quantity\"");
                    table.ForeignKey(
                        name: "FK_mob_loot_drops_item_templates_item_template_id",
                        column: x => x.item_template_id,
                        principalTable: "item_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mob_loot_drops_mob_templates_mob_template_id",
                        column: x => x.mob_template_id,
                        principalTable: "mob_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wandering_shop_stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: true),
                    price_in_gold = table.Column<long>(type: "bigint", nullable: false),
                    price_modifier_pct = table.Column<decimal>(type: "numeric", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wandering_shop_stock", x => x.id);
                    table.ForeignKey(
                        name: "FK_wandering_shop_stock_item_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "item_templates",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "settlement_members",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    settlement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_registered_for_war = table.Column<bool>(type: "boolean", nullable: false),
                    contribution_gold = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_members", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_settlement_members_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_settlement_members_settlements_settlement_id",
                        column: x => x.settlement_id,
                        principalTable: "settlements",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "settlement_upgrades",
                columns: table => new
                {
                    settlement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    upgrade_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_upgrades", x => new { x.settlement_id, x.upgrade_type });
                    table.ForeignKey(
                        name: "FK_settlement_upgrades_settlements_settlement_id",
                        column: x => x.settlement_id,
                        principalTable: "settlements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loadouts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: true),
                    role_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    weapon_inv_id = table.Column<Guid>(type: "uuid", nullable: true),
                    armor_inv_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pet_inv_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadouts", x => x.id);
                    table.ForeignKey(
                        name: "FK_loadouts_inventory_armor_inv_id",
                        column: x => x.armor_inv_id,
                        principalTable: "inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loadouts_inventory_pet_inv_id",
                        column: x => x.pet_inv_id,
                        principalTable: "inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loadouts_inventory_weapon_inv_id",
                        column: x => x.weapon_inv_id,
                        principalTable: "inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loadouts_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "player_unlocked_nodes",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    node_id = table.Column<int>(type: "integer", nullable: false),
                    loadout_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_unlocked_nodes", x => new { x.player_id, x.node_id, x.loadout_id });
                    table.ForeignKey(
                        name: "FK_player_unlocked_nodes_loadouts_loadout_id",
                        column: x => x.loadout_id,
                        principalTable: "loadouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_unlocked_nodes_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_unlocked_nodes_skill_trees_node_id",
                        column: x => x.node_id,
                        principalTable: "skill_trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_email",
                table: "accounts",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_biome_mobs_mob_template_id",
                table: "biome_mobs",
                column: "mob_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_biomes_code",
                table: "biomes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_headquarter_npcs_player_id",
                table: "headquarter_npcs",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_player_id",
                table: "inventory",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_template_id",
                table: "inventory",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_code",
                table: "item_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_templates_category_id_tier",
                table: "item_templates",
                columns: new[] { "category_id", "tier" });

            migrationBuilder.CreateIndex(
                name: "IX_item_templates_code",
                table: "item_templates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadouts_armor_inv_id",
                table: "loadouts",
                column: "armor_inv_id");

            migrationBuilder.CreateIndex(
                name: "IX_loadouts_pet_inv_id",
                table: "loadouts",
                column: "pet_inv_id");

            migrationBuilder.CreateIndex(
                name: "IX_loadouts_player_id",
                table: "loadouts",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_loadouts_weapon_inv_id",
                table: "loadouts",
                column: "weapon_inv_id");

            migrationBuilder.CreateIndex(
                name: "IX_mob_loot_drops_item_template_id",
                table: "mob_loot_drops",
                column: "item_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_mob_loot_drops_mob_template_id_item_template_id",
                table: "mob_loot_drops",
                columns: new[] { "mob_template_id", "item_template_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mob_templates_code",
                table: "mob_templates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_unlocked_nodes_loadout_id",
                table: "player_unlocked_nodes",
                column: "loadout_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_unlocked_nodes_node_id",
                table: "player_unlocked_nodes",
                column: "node_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_account_id",
                table: "players",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_username",
                table: "players",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pvp_logs_attacker_id",
                table: "pvp_logs",
                column: "attacker_id");

            migrationBuilder.CreateIndex(
                name: "IX_pvp_logs_defender_id",
                table: "pvp_logs",
                column: "defender_id");

            migrationBuilder.CreateIndex(
                name: "IX_settlement_members_settlement_id",
                table: "settlement_members",
                column: "settlement_id");

            migrationBuilder.CreateIndex(
                name: "IX_settlements_leader_id",
                table: "settlements",
                column: "leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_wandering_shop_stock_template_id",
                table: "wandering_shop_stock",
                column: "template_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "biome_mobs");

            migrationBuilder.DropTable(
                name: "headquarter_npcs");

            migrationBuilder.DropTable(
                name: "item_combat_profiles");

            migrationBuilder.DropTable(
                name: "mob_loot_drops");

            migrationBuilder.DropTable(
                name: "player_activity_states");

            migrationBuilder.DropTable(
                name: "player_professions");

            migrationBuilder.DropTable(
                name: "player_stats");

            migrationBuilder.DropTable(
                name: "player_unlocked_nodes");

            migrationBuilder.DropTable(
                name: "pvp_logs");

            migrationBuilder.DropTable(
                name: "settlement_members");

            migrationBuilder.DropTable(
                name: "settlement_upgrades");

            migrationBuilder.DropTable(
                name: "wandering_shop_stock");

            migrationBuilder.DropTable(
                name: "biomes");

            migrationBuilder.DropTable(
                name: "mob_templates");

            migrationBuilder.DropTable(
                name: "loadouts");

            migrationBuilder.DropTable(
                name: "skill_trees");

            migrationBuilder.DropTable(
                name: "settlements");

            migrationBuilder.DropTable(
                name: "inventory");

            migrationBuilder.DropTable(
                name: "item_templates");

            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "item_categories");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
