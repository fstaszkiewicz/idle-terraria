using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleTerraria.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillPointsToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "skill_points",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_players_username",
                table: "players",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_username",
                table: "players");

            migrationBuilder.DropColumn(
                name: "skill_points",
                table: "players");
        }
    }
}
