using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAllUlduarBossNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Encounters""
                SET ""EncounterName"" = CASE ""EncounterEntry""
                    WHEN '33113' THEN 'Огненный Левиафан'
                    WHEN '33114' THEN 'Повелитель Горнов Игнис'
                    WHEN '33115' THEN 'Острокрылая'
                    WHEN '33116' THEN 'Разрушитель XT-002'
                    WHEN '33117' THEN 'Железное собрание'
                    WHEN '33118' THEN 'Кологарн'
                    WHEN '33119' THEN 'Ауриайя'
                    WHEN '33120' THEN 'Мимирон'
                    WHEN '33121' THEN 'Фрейя'
                    WHEN '33122' THEN 'Торим'
                    WHEN '33123' THEN 'Ходир'
                    WHEN '33124' THEN 'Генерал Везакс'
                    WHEN '33125' THEN 'Йогг-Сарон'
                    WHEN '33126' THEN 'Алгалон Наблюдатель'
                    WHEN '32845' THEN 'Ходир'
                    WHEN '32857' THEN 'Железное собрание'
                    WHEN '32865' THEN 'Торим'
                    WHEN '32906' THEN 'Фрейя'
                    WHEN '32930' THEN 'Кологарн'
                    WHEN '33186' THEN 'Острокрылая'
                    WHEN '33271' THEN 'Генерал Везакс'
                    WHEN '33288' THEN 'Йогг-Сарон'
                    WHEN '33293' THEN 'Разрушитель XT-002'
                    WHEN '33350' THEN 'Мимирон'
                    WHEN '33515' THEN 'Ауриайя'
                    ELSE ""EncounterName""
                END
                WHERE ""EncounterEntry"" IN ('33113', '33114', '33115', '33116', '33117', '33118', '33119', '33120', '33121', '33122', '33123', '33124', '33125', '33126', '32845', '32857', '32865', '32906', '32930', '33186', '33271', '33288', '33293', '33350', '33515');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
