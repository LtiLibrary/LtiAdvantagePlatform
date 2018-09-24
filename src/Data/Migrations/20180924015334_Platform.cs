using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class Platform : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlatformId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    PlatformId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.PlatformId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PlatformId",
                table: "AspNetUsers",
                column: "PlatformId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Platforms_PlatformId",
                table: "AspNetUsers",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "PlatformId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Platforms_PlatformId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PlatformId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "AspNetUsers");
        }
    }
}
