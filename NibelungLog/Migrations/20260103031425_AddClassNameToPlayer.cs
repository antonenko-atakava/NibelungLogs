using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class AddClassNameToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Players""
                SET ""ClassName"" = CASE ""CharacterClass""
                    WHEN '1' THEN 'Воин'
                    WHEN '2' THEN 'Паладин'
                    WHEN '3' THEN 'Охотник'
                    WHEN '4' THEN 'Разбойник'
                    WHEN '5' THEN 'Жрец'
                    WHEN '6' THEN 'Рыцарь смерти'
                    WHEN '7' THEN 'Шаман'
                    WHEN '8' THEN 'Маг'
                    WHEN '9' THEN 'Чернокнижник'
                    WHEN '11' THEN 'Друид'
                    ELSE NULL
                END
                WHERE ""ClassName"" IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""CharacterSpecs""
                SET ""Name"" = CASE 
                    WHEN ""CharacterClass"" = '1' AND ""Spec"" = '0' THEN 'Оружие'
                    WHEN ""CharacterClass"" = '1' AND ""Spec"" = '1' THEN 'Неистовство'
                    WHEN ""CharacterClass"" = '1' AND ""Spec"" = '2' THEN 'Защита'
                    WHEN ""CharacterClass"" = '2' AND ""Spec"" = '0' THEN 'Свет'
                    WHEN ""CharacterClass"" = '2' AND ""Spec"" = '1' THEN 'Защита'
                    WHEN ""CharacterClass"" = '2' AND ""Spec"" = '2' THEN 'Воздаяние'
                    WHEN ""CharacterClass"" = '3' AND ""Spec"" = '0' THEN 'Повелитель зверей'
                    WHEN ""CharacterClass"" = '3' AND ""Spec"" = '1' THEN 'Стрельба'
                    WHEN ""CharacterClass"" = '3' AND ""Spec"" = '2' THEN 'Выживание'
                    WHEN ""CharacterClass"" = '4' AND ""Spec"" = '0' THEN 'Убийство'
                    WHEN ""CharacterClass"" = '4' AND ""Spec"" = '1' THEN 'Бой'
                    WHEN ""CharacterClass"" = '4' AND ""Spec"" = '2' THEN 'Скрытность'
                    WHEN ""CharacterClass"" = '5' AND ""Spec"" = '0' THEN 'Послушание'
                    WHEN ""CharacterClass"" = '5' AND ""Spec"" = '1' THEN 'Свет'
                    WHEN ""CharacterClass"" = '5' AND ""Spec"" = '2' THEN 'Тьма'
                    WHEN ""CharacterClass"" = '6' AND ""Spec"" = '0' THEN 'Кровь'
                    WHEN ""CharacterClass"" = '6' AND ""Spec"" = '1' THEN 'Лед'
                    WHEN ""CharacterClass"" = '6' AND ""Spec"" = '2' THEN 'Нечестивость'
                    WHEN ""CharacterClass"" = '7' AND ""Spec"" = '0' THEN 'Стихии'
                    WHEN ""CharacterClass"" = '7' AND ""Spec"" = '1' THEN 'Улучшение'
                    WHEN ""CharacterClass"" = '7' AND ""Spec"" = '2' THEN 'Исцеление'
                    WHEN ""CharacterClass"" = '8' AND ""Spec"" = '0' THEN 'Тайная магия'
                    WHEN ""CharacterClass"" = '8' AND ""Spec"" = '1' THEN 'Огонь'
                    WHEN ""CharacterClass"" = '8' AND ""Spec"" = '2' THEN 'Лед'
                    WHEN ""CharacterClass"" = '9' AND ""Spec"" = '0' THEN 'Колдовство'
                    WHEN ""CharacterClass"" = '9' AND ""Spec"" = '1' THEN 'Демонология'
                    WHEN ""CharacterClass"" = '9' AND ""Spec"" = '2' THEN 'Разрушение'
                    WHEN ""CharacterClass"" = '11' AND ""Spec"" = '0' THEN 'Баланс'
                    WHEN ""CharacterClass"" = '11' AND ""Spec"" = '1' THEN 'Сила зверя'
                    WHEN ""CharacterClass"" = '11' AND ""Spec"" = '2' THEN 'Исцеление'
                    ELSE NULL
                END
                WHERE ""Name"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "Players");
        }
    }
}
