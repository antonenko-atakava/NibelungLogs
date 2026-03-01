using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class FixAlgalon32871Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Encounters""
                SET ""EncounterName"" = 'Алгалон Наблюдатель'
                WHERE ""EncounterEntry"" = '32871' AND (""EncounterName"" IS NULL OR ""EncounterName"" != 'Алгалон Наблюдатель');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
