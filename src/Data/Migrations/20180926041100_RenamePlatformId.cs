using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class RenamePlatformId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlatformId",
                table: "Platforms",
                newName: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Platforms",
                newName: "PlatformId");
        }
    }
}
