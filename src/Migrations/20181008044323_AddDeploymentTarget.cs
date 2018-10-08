using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class AddDeploymentTarget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_Courses_CourseId",
                table: "Deployments");

            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_Platforms_PlatformId",
                table: "Deployments");

            migrationBuilder.DropIndex(
                name: "IX_Deployments_CourseId",
                table: "Deployments");

            migrationBuilder.DropIndex(
                name: "IX_Deployments_PlatformId",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "Deployments");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Tools",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToolPlacement",
                table: "Deployments",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToolPlacement",
                table: "Deployments");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Tools",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "Deployments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlatformId",
                table: "Deployments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_CourseId",
                table: "Deployments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_PlatformId",
                table: "Deployments",
                column: "PlatformId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_Courses_CourseId",
                table: "Deployments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_Platforms_PlatformId",
                table: "Deployments",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
