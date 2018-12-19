using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class AddResourceLinkDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradebookColumns_ResourceLinks_ResourceLinkId",
                table: "GradebookColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceLinks_Tools_ToolId",
                table: "ResourceLinks");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ResourceLinks",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GradebookColumns_ResourceLinks_ResourceLinkId",
                table: "GradebookColumns",
                column: "ResourceLinkId",
                principalTable: "ResourceLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_GradebookColumns_ResourceLinks_ResourceLinkId",
                table: "GradebookColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceLinks_Tools_ToolId",
                table: "ResourceLinks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ResourceLinks");

            migrationBuilder.AddForeignKey(
                name: "FK_GradebookColumns_ResourceLinks_ResourceLinkId",
                table: "GradebookColumns",
                column: "ResourceLinkId",
                principalTable: "ResourceLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceLinks_Tools_ToolId",
                table: "ResourceLinks",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
