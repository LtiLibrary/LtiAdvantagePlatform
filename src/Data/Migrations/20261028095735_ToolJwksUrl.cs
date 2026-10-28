using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvantagePlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class ToolJwksUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JwksUrl",
                table: "Tools",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwksUrl",
                table: "Tools");
        }
    }
}
