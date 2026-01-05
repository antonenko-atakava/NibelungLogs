using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class AddEncounterNameAndRaidDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncounterName",
                table: "Encounters",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""RaidTypes""
                SET ""Name"" = CASE 
                    WHEN ""Map"" = '533' AND ""Difficulty"" = '1' THEN 'Наксрамас 25'
                    WHEN ""Map"" = '533' AND ""Difficulty"" = '0' THEN 'Наксрамас 10'
                    WHEN ""Map"" = '615' AND ""Difficulty"" = '1' THEN 'Око Вечности 25'
                    WHEN ""Map"" = '615' AND ""Difficulty"" = '0' THEN 'Око Вечности 10'
                    ELSE ""Name""
                END;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Encounters""
                SET ""EncounterName"" = CASE ""EncounterEntry""
                    WHEN '15956' THEN 'Ануб''Рекан'
                    WHEN '30549' THEN 'Барон Ривендер'
                    WHEN '15953' THEN 'Великая вдова Фарлина'
                    WHEN '15932' THEN 'Глут'
                    WHEN '16060' THEN 'Готик Жнец'
                    WHEN '15931' THEN 'Гроббулус'
                    WHEN '16061' THEN 'Инструктор Разувий'
                    WHEN '15990' THEN 'Кел''Тузад'
                    WHEN '16065' THEN 'Леди Бломе'
                    WHEN '16028' THEN 'Лоскутик'
                    WHEN '16011' THEN 'Лотхиб'
                    WHEN '15952' THEN 'Мексна'
                    WHEN '15954' THEN 'Нот Чумной'
                    WHEN '15989' THEN 'Сапфирон'
                    WHEN '16063' THEN 'Сэр Зелиек'
                    WHEN '15928' THEN 'Таддиус'
                    WHEN '16064' THEN 'Тан Кортазз'
                    WHEN '15936' THEN 'Хейган Нечестивый'
                    ELSE NULL
                END
                WHERE ""EncounterName"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncounterName",
                table: "Encounters");
        }
    }
}
