using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class RenameField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISClientId",
                table: "Tools",
                newName: "IdentSvrClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdentSvrClientId",
                table: "Tools",
                newName: "ISClientId");
        }
    }
}
