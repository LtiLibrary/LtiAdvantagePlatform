using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class RemoveRedundantResourceLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceLinks_AspNetUsers_AdvantagePlatformUserId",
                table: "ResourceLinks");

            migrationBuilder.DropIndex(
                name: "IX_ResourceLinks_AdvantagePlatformUserId",
                table: "ResourceLinks");

            migrationBuilder.DropColumn(
                name: "AdvantagePlatformUserId",
                table: "ResourceLinks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdvantagePlatformUserId",
                table: "ResourceLinks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceLinks_AdvantagePlatformUserId",
                table: "ResourceLinks",
                column: "AdvantagePlatformUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceLinks_AspNetUsers_AdvantagePlatformUserId",
                table: "ResourceLinks",
                column: "AdvantagePlatformUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
