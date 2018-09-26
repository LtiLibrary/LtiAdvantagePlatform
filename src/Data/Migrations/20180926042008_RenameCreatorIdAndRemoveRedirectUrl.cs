using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class RenameCreatorIdAndRemoveRedirectUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectUrl",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Clients",
                newName: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Clients",
                newName: "CreatorId");

            migrationBuilder.AddColumn<string>(
                name: "RedirectUrl",
                table: "Clients",
                nullable: false,
                defaultValue: "");
        }
    }
}
