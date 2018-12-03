using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class AddGradebookRow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GradebookRows",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ActivityProgress = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    GradingProgress = table.Column<int>(nullable: false),
                    ScoreGiven = table.Column<double>(nullable: false),
                    ScoreMaximum = table.Column<double>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    PersonId = table.Column<string>(nullable: true),
                    GradebookColumnId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradebookRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradebookRows_GradebookColumns_GradebookColumnId",
                        column: x => x.GradebookColumnId,
                        principalTable: "GradebookColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GradebookRows_GradebookColumnId",
                table: "GradebookRows",
                column: "GradebookColumnId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GradebookRows");
        }
    }
}
