using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class ForceFixIgnisName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Encounters""
                SET ""EncounterName"" = 'Повелитель Горнов Игнис'
                WHERE ""EncounterEntry"" = '33114';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
