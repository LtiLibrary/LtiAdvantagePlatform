using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvantagePlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class CourseGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseGroupSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Tag = table.Column<string>(type: "TEXT", nullable: true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroupSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseGroupSets_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Tag = table.Column<string>(type: "TEXT", nullable: true),
                    GroupSetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseGroups_CourseGroupSets_GroupSetId",
                        column: x => x.GroupSetId,
                        principalTable: "CourseGroupSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseGroupPerson",
                columns: table => new
                {
                    CourseGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroupPerson", x => new { x.CourseGroupId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_CourseGroupPerson_CourseGroups_CourseGroupId",
                        column: x => x.CourseGroupId,
                        principalTable: "CourseGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseGroupPerson_People_MembersId",
                        column: x => x.MembersId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroupPerson_MembersId",
                table: "CourseGroupPerson",
                column: "MembersId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroups_GroupSetId",
                table: "CourseGroups",
                column: "GroupSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroupSets_CourseId",
                table: "CourseGroupSets",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseGroupPerson");

            migrationBuilder.DropTable(
                name: "CourseGroups");

            migrationBuilder.DropTable(
                name: "CourseGroupSets");
        }
    }
}
