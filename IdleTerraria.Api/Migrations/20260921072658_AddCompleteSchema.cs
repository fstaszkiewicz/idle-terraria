using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdleTerraria.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompleteSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "stat_vitality",
                table: "player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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
                name: "item_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    equipable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_categories", x => x.id);
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
                name: "item_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tier = table.Column<int>(type: "integer", nullable: false),
                    base_value = table.Column<int>(type: "integer", nullable: false),
                    attack_speed = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_item_templates_item_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "item_categories",
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
                name: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: true),
                    template_id = table.Column<int>(type: "integer", nullable: true),
                    prefix = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    upgrade_level = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventory_item_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "item_templates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_inventory_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id");
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
                name: "IX_item_templates_category_id",
                table: "item_templates",
                column: "category_id");

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
                name: "IX_player_unlocked_nodes_loadout_id",
                table: "player_unlocked_nodes",
                column: "loadout_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_unlocked_nodes_node_id",
                table: "player_unlocked_nodes",
                column: "node_id");

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
                name: "headquarter_npcs");

            migrationBuilder.DropTable(
                name: "player_activity_states");

            migrationBuilder.DropTable(
                name: "player_professions");

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
                name: "item_categories");

            migrationBuilder.DropColumn(
                name: "stat_vitality",
                table: "player_stats");
        }
    }
}
