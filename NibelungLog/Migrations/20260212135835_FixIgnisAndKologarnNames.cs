using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class FixIgnisAndKologarnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Encounters""
                SET ""EncounterName"" = CASE ""EncounterEntry""
                    WHEN '33118' THEN 'Повелитель Горнов Игнис'
                    WHEN '32930' THEN 'Кологарн'
                    ELSE ""EncounterName""
                END
                WHERE ""EncounterEntry"" IN ('33118', '32930');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
