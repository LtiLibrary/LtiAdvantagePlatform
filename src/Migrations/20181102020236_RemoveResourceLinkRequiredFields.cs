using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class RemoveResourceLinkRequiredFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ResourceLinks",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "LinkContext",
                table: "ResourceLinks",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ResourceLinks",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LinkContext",
                table: "ResourceLinks",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
