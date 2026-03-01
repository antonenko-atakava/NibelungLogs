using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NibelungLog.Migrations
{
    /// <inheritdoc />
    public partial class AddObsidianSanctumRaidType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""RaidTypes"" (""Name"", ""Map"", ""Difficulty"", ""InstanceType"")
                SELECT 'Обсидиановое святилище 25', '615', '1', '6'
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""RaidTypes"" 
                    WHERE ""Map"" = '615' AND ""Difficulty"" = '1' AND ""InstanceType"" = '6'
                );
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""RaidTypes""
                SET ""Name"" = 'Обсидиановое святилище 25'
                WHERE ""Map"" = '615' AND ""Difficulty"" = '1' AND ""InstanceType"" = '6';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
