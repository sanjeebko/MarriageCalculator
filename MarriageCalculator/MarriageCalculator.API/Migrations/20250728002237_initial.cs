using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarriageCalculator.API.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Murder = table.Column<bool>(type: "bit", nullable: false),
                    Kidnap = table.Column<bool>(type: "bit", nullable: false),
                    SeenPoint = table.Column<int>(type: "int", nullable: false),
                    UnseenPoint = table.Column<int>(type: "int", nullable: false),
                    PointRate = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Dublee = table.Column<bool>(type: "bit", nullable: false),
                    DubleePointLess = table.Column<bool>(type: "bit", nullable: false),
                    DubleePointBonus = table.Column<int>(type: "int", nullable: false),
                    FoulPoint = table.Column<int>(type: "int", nullable: false),
                    FoulPointBonus = table.Column<int>(type: "int", nullable: false),
                    Audio = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarriageGameSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    GameSettingsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarriageGameSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarriageGameSet_GameSettings_GameSettingsId",
                        column: x => x.GameSettingsId,
                        principalTable: "GameSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarriageGameRound",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    MarriageGameSetId = table.Column<int>(type: "int", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarriageGameRound", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarriageGameRound_MarriageGameSet_MarriageGameSetId",
                        column: x => x.MarriageGameSetId,
                        principalTable: "MarriageGameSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarriageGameSetPlayer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarriageGameSetId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarriageGameSetPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarriageGameSetPlayer_MarriageGameSet_MarriageGameSetId",
                        column: x => x.MarriageGameSetId,
                        principalTable: "MarriageGameSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarriageGameSetPlayer_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarriageGame",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    MarriageGameRoundId = table.Column<int>(type: "int", nullable: false),
                    WinnerId = table.Column<int>(type: "int", nullable: false),
                    DealerId = table.Column<int>(type: "int", nullable: false),
                    TotalMaal = table.Column<int>(type: "int", nullable: false),
                    ClosedRound = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarriageGame", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarriageGame_MarriageGameRound_MarriageGameRoundId",
                        column: x => x.MarriageGameRoundId,
                        principalTable: "MarriageGameRound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarriageGame_Player_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarriageGame_Player_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarriageGameScore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarriageGameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Seen = table.Column<bool>(type: "bit", nullable: false),
                    Playing = table.Column<bool>(type: "bit", nullable: false),
                    Maal = table.Column<int>(type: "int", nullable: false),
                    BonusPoint = table.Column<int>(type: "int", nullable: false),
                    Duply = table.Column<bool>(type: "bit", nullable: false),
                    Winner = table.Column<bool>(type: "bit", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MoneyWon = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                    Deal = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarriageGameScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarriageGameScore_MarriageGame_MarriageGameId",
                        column: x => x.MarriageGameId,
                        principalTable: "MarriageGame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarriageGameScore_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGame_DealerId",
                table: "MarriageGame",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGame_MarriageGameRoundId",
                table: "MarriageGame",
                column: "MarriageGameRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGame_WinnerId",
                table: "MarriageGame",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGameRound_MarriageGameSetId",
                table: "MarriageGameRound",
                column: "MarriageGameSetId");

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGameScore_MarriageGameId_PlayerId",
                table: "MarriageGameScore",
                columns: new[] { "MarriageGameId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGameScore_PlayerId",
                table: "MarriageGameScore",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGameSet_GameSettingsId",
                table: "MarriageGameSet",
                column: "GameSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGameSetPlayer_MarriageGameSetId_PlayerId",
                table: "MarriageGameSetPlayer",
                columns: new[] { "MarriageGameSetId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarriageGameSetPlayer_PlayerId",
                table: "MarriageGameSetPlayer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_Name_Email",
                table: "Player",
                columns: new[] { "Name", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarriageGameScore");

            migrationBuilder.DropTable(
                name: "MarriageGameSetPlayer");

            migrationBuilder.DropTable(
                name: "MarriageGame");

            migrationBuilder.DropTable(
                name: "MarriageGameRound");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "MarriageGameSet");

            migrationBuilder.DropTable(
                name: "GameSettings");
        }
    }
}
