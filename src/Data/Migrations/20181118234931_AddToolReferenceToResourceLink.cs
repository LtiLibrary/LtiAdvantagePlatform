using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class AddToolReferenceToResourceLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ResourceLinks_ToolId",
                table: "ResourceLinks",
                column: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceLinks_Tools_ToolId",
                table: "ResourceLinks",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceLinks_Tools_ToolId",
                table: "ResourceLinks");

            migrationBuilder.DropIndex(
                name: "IX_ResourceLinks_ToolId",
                table: "ResourceLinks");
        }
    }
}
