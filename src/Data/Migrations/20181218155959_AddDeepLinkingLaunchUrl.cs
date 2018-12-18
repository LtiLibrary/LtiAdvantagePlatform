using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class AddDeepLinkingLaunchUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeepLinkingLaunchUrl",
                table: "Tools",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeepLinkingLaunchUrl",
                table: "Tools");
        }
    }
}
