using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CharacterSpecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterClass = table.Column<string>(type: "text", nullable: false),
                    Spec = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSpecs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterGuid = table.Column<string>(type: "text", nullable: false),
                    CharacterName = table.Column<string>(type: "text", nullable: false),
                    CharacterRace = table.Column<string>(type: "text", nullable: false),
                    CharacterClass = table.Column<string>(type: "text", nullable: false),
                    CharacterGender = table.Column<string>(type: "text", nullable: false),
                    CharacterLevel = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaidTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Map = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    InstanceType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Raids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RaidId = table.Column<string>(type: "text", nullable: false),
                    RaidTypeId = table.Column<int>(type: "integer", nullable: false),
                    GuildName = table.Column<string>(type: "text", nullable: false),
                    LeaderName = table.Column<string>(type: "text", nullable: false),
                    LeaderGuid = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalTime = table.Column<long>(type: "bigint", nullable: false),
                    TotalDamage = table.Column<long>(type: "bigint", nullable: false),
                    TotalHealing = table.Column<long>(type: "bigint", nullable: false),
                    AverageGearScore = table.Column<string>(type: "text", nullable: false),
                    MaxGearScore = table.Column<string>(type: "text", nullable: false),
                    Wipes = table.Column<int>(type: "integer", nullable: false),
                    CompletedBosses = table.Column<int>(type: "integer", nullable: false),
                    TotalBosses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Raids_RaidTypes_RaidTypeId",
                        column: x => x.RaidTypeId,
                        principalTable: "RaidTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RaidId = table.Column<int>(type: "integer", nullable: false),
                    EncounterEntry = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    TotalDamage = table.Column<long>(type: "bigint", nullable: false),
                    TotalHealing = table.Column<long>(type: "bigint", nullable: false),
                    Tanks = table.Column<int>(type: "integer", nullable: false),
                    Healers = table.Column<int>(type: "integer", nullable: false),
                    DamageDealers = table.Column<int>(type: "integer", nullable: false),
                    AverageGearScore = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encounters_Raids_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEncounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    EncounterId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSpecId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    DamageDone = table.Column<long>(type: "bigint", nullable: false),
                    HealingDone = table.Column<long>(type: "bigint", nullable: false),
                    AbsorbProvided = table.Column<long>(type: "bigint", nullable: false),
                    Dps = table.Column<double>(type: "double precision", nullable: false),
                    MaxAverageGearScore = table.Column<string>(type: "text", nullable: false),
                    MaxGearScore = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEncounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerEncounters_CharacterSpecs_CharacterSpecId",
                        column: x => x.CharacterSpecId,
                        principalTable: "CharacterSpecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerEncounters_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerEncounters_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpecs_CharacterClass_Spec",
                table: "CharacterSpecs",
                columns: new[] { "CharacterClass", "Spec" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_RaidId_EncounterEntry_StartTime",
                table: "Encounters",
                columns: new[] { "RaidId", "EncounterEntry", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEncounters_CharacterSpecId",
                table: "PlayerEncounters",
                column: "CharacterSpecId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEncounters_EncounterId",
                table: "PlayerEncounters",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEncounters_PlayerId_EncounterId",
                table: "PlayerEncounters",
                columns: new[] { "PlayerId", "EncounterId" });

            migrationBuilder.CreateIndex(
                name: "IX_Players_CharacterGuid",
                table: "Players",
                column: "CharacterGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_CharacterName",
                table: "Players",
                column: "CharacterName");

            migrationBuilder.CreateIndex(
                name: "IX_Raids_RaidId",
                table: "Raids",
                column: "RaidId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Raids_RaidTypeId",
                table: "Raids",
                column: "RaidTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Raids_StartTime",
                table: "Raids",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_RaidTypes_Map_Difficulty_InstanceType",
                table: "RaidTypes",
                columns: new[] { "Map", "Difficulty", "InstanceType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerEncounters");

            migrationBuilder.DropTable(
                name: "CharacterSpecs");

            migrationBuilder.DropTable(
                name: "Encounters");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Raids");

            migrationBuilder.DropTable(
                name: "RaidTypes");
        }
    }
}
