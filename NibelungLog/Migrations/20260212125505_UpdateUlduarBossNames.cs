using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUlduarBossNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Encounters""
                SET ""EncounterName"" = CASE ""EncounterEntry""
                    WHEN '33114' THEN 'Повелитель Горнов Игнис'
                    WHEN '33115' THEN 'Острокрылая'
                    WHEN '33117' THEN 'Железное собрание'
                    WHEN '33124' THEN 'Генерал Везакс'
                    WHEN '33126' THEN 'Алгалон Наблюдатель'
                    ELSE ""EncounterName""
                END
                WHERE ""EncounterEntry"" IN ('33114', '33115', '33117', '33124', '33126');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
