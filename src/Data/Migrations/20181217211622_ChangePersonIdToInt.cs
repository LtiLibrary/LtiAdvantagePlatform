using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class ChangePersonIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradebookRows_GradebookColumns_GradebookColumnId",
                table: "GradebookRows");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "GradebookRows",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GradebookRows_GradebookColumns_GradebookColumnId",
                table: "GradebookRows",
                column: "GradebookColumnId",
                principalTable: "GradebookColumns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradebookRows_GradebookColumns_GradebookColumnId",
                table: "GradebookRows");

            migrationBuilder.AlterColumn<string>(
                name: "PersonId",
                table: "GradebookRows",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_GradebookRows_GradebookColumns_GradebookColumnId",
                table: "GradebookRows",
                column: "GradebookColumnId",
                principalTable: "GradebookColumns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
