using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexPlayerEncounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerEncounters_PlayerId_EncounterId",
                table: "PlayerEncounters");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEncounters_PlayerId_EncounterId",
                table: "PlayerEncounters",
                columns: new[] { "PlayerId", "EncounterId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerEncounters_PlayerId_EncounterId",
                table: "PlayerEncounters");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEncounters_PlayerId_EncounterId",
                table: "PlayerEncounters",
                columns: new[] { "PlayerId", "EncounterId" });
        }
    }
}
