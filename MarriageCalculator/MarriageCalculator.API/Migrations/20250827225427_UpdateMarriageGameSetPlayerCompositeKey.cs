using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarriageCalculator.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarriageGameSetPlayerCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MarriageGameSetPlayer",
                table: "MarriageGameSetPlayer");

            migrationBuilder.DropIndex(
                name: "IX_MarriageGameSetPlayer_MarriageGameSetId_PlayerId",
                table: "MarriageGameSetPlayer");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MarriageGameSetPlayer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MarriageGameSetPlayer",
                table: "MarriageGameSetPlayer",
                columns: new[] { "MarriageGameSetId", "PlayerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MarriageGameSetPlayer",
                table: "MarriageGameSetPlayer");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MarriageGameSetPlayer",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MarriageGameSetPlayer",
                table: "MarriageGameSetPlayer",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGameSetPlayer_MarriageGameSetId_PlayerId",
                table: "MarriageGameSetPlayer",
                columns: new[] { "MarriageGameSetId", "PlayerId" },
                unique: true);
        }
    }
}
