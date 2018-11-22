using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class AddCustomProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomProperties",
                table: "Tools",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomProperties",
                table: "ResourceLinks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomProperties",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "CustomProperties",
                table: "ResourceLinks");
        }
    }
}
