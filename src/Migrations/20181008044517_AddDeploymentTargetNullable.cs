using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class AddDeploymentTargetNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ToolPlacement",
                table: "Deployments",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ToolPlacement",
                table: "Deployments",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
