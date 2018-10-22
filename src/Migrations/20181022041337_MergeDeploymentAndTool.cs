using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class MergeDeploymentAndTool : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_Tools_ToolId",
                table: "Deployments");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropIndex(
                name: "IX_Deployments_ToolId",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "ToolId",
                table: "Deployments");

            migrationBuilder.AddColumn<string>(
                name: "ToolName",
                table: "Deployments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToolUrl",
                table: "Deployments",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToolName",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "ToolUrl",
                table: "Deployments");

            migrationBuilder.AddColumn<int>(
                name: "ToolId",
                table: "Deployments",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_ToolId",
                table: "Deployments",
                column: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_Tools_ToolId",
                table: "Deployments",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
