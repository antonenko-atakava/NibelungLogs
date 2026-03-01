using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class AddEyeOfEternityRaidType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""RaidTypes"" (""Name"", ""Map"", ""Difficulty"", ""InstanceType"")
                SELECT 'Око Вечности 25', '616', '1', '6'
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""RaidTypes"" 
                    WHERE ""Map"" = '616' AND ""Difficulty"" = '1' AND ""InstanceType"" = '6'
                );
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""RaidTypes""
                SET ""Name"" = 'Око Вечности 25'
                WHERE ""Map"" = '616' AND ""Difficulty"" = '1' AND ""InstanceType"" = '6';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
