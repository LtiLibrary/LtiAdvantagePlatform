using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class AddToolPrivateKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToolIssuer",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "ToolJsonWebKeysUrl",
                table: "Tools");

            migrationBuilder.RenameColumn(
                name: "ToolUrl",
                table: "Tools",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "ToolName",
                table: "Tools",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "IdentSvrClientId",
                table: "Tools",
                newName: "IdentityServerClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Tools",
                newName: "ToolUrl");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tools",
                newName: "ToolName");

            migrationBuilder.RenameColumn(
                name: "IdentityServerClientId",
                table: "Tools",
                newName: "IdentSvrClientId");

            migrationBuilder.AddColumn<string>(
                name: "ToolIssuer",
                table: "Tools",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToolJsonWebKeysUrl",
                table: "Tools",
                nullable: true);
        }
    }
}
