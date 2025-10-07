using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarriageCalculator.API.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlayerNameEmailUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Player_Name_Email",
                table: "Player");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Player_Name_Email",
                table: "Player",
                columns: new[] { "Name", "Email" },
                unique: true);
        }
    }
}
