using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvantagePlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeepLinkingContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentTarget",
                table: "ResourceLinks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IframeHeight",
                table: "ResourceLinks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LinkedContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    Html = table.Column<string>(type: "TEXT", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentTarget = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlatformId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkedContent_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LinkedContent_Platforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinkedContent_CourseId",
                table: "LinkedContent",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedContent_PlatformId",
                table: "LinkedContent",
                column: "PlatformId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkedContent");

            migrationBuilder.DropColumn(
                name: "DocumentTarget",
                table: "ResourceLinks");

            migrationBuilder.DropColumn(
                name: "IframeHeight",
                table: "ResourceLinks");
        }
    }
}
