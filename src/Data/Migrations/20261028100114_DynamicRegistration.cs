using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvantagePlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class DynamicRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedirectUris",
                table: "Tools",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ToolRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    InitiationUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ToolId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolRegistrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToolRegistrations_Token",
                table: "ToolRegistrations",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToolRegistrations");

            migrationBuilder.DropColumn(
                name: "RedirectUris",
                table: "Tools");
        }
    }
}
