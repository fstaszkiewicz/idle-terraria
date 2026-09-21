using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleTerraria.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<long>(type: "bigint", nullable: false),
                    gold = table.Column<long>(type: "bigint", nullable: false),
                    stardust = table.Column<int>(type: "integer", nullable: false),
                    energy = table.Column<int>(type: "integer", nullable: false),
                    arena_elo = table.Column<int>(type: "integer", nullable: false),
                    current_biome_id = table.Column<int>(type: "integer", nullable: true),
                    stats_bought_n = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_stats",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stat_strength = table.Column<int>(type: "integer", nullable: false),
                    stat_dexterity = table.Column<int>(type: "integer", nullable: false),
                    stat_luck = table.Column<int>(type: "integer", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_stats");

            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
