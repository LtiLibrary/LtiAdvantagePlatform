using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class UpdateResourceLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ResourceLinks");

            migrationBuilder.DropColumn(
                name: "DeploymentId",
                table: "ResourceLinks");

            migrationBuilder.DropColumn(
                name: "ToolName",
                table: "ResourceLinks");

            migrationBuilder.RenameColumn(
                name: "ToolUrl",
                table: "ResourceLinks",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "ToolPlacement",
                table: "ResourceLinks",
                newName: "ToolId");

            migrationBuilder.AddColumn<int>(
                name: "LinkContext",
                table: "ResourceLinks",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkContext",
                table: "ResourceLinks");

            migrationBuilder.RenameColumn(
                name: "ToolId",
                table: "ResourceLinks",
                newName: "ToolPlacement");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "ResourceLinks",
                newName: "ToolUrl");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ResourceLinks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeploymentId",
                table: "ResourceLinks",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToolName",
                table: "ResourceLinks",
                nullable: false,
                defaultValue: "");
        }
    }
}
