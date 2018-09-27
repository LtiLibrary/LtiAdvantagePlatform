using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class AddPlatformFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Platforms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Platforms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Guid",
                table: "Platforms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Platforms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductFamilyCode",
                table: "Platforms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Platforms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Platforms",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "ProductFamilyCode",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Platforms");
        }
    }
}
