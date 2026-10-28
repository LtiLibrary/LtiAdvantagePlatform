using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvantagePlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class Proctoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProctored",
                table: "ResourceLinks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProctoringSettings",
                table: "ResourceLinks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProctoringAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResourceLinkId = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionData = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraTime = table.Column<int>(type: "INTEGER", nullable: false),
                    EndAssessmentReturn = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerifiedUserName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProctoringAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProctoringAttempts_ResourceLinks_ResourceLinkId",
                        column: x => x.ResourceLinkId,
                        principalTable: "ResourceLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProctoringEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProctoringAttemptId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IncidentTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IncidentSeverity = table.Column<double>(type: "REAL", nullable: true),
                    ExtraTime = table.Column<int>(type: "INTEGER", nullable: true),
                    ReasonCode = table.Column<string>(type: "TEXT", nullable: true),
                    ReasonMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ResultingStatus = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProctoringEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProctoringEvents_ProctoringAttempts_ProctoringAttemptId",
                        column: x => x.ProctoringAttemptId,
                        principalTable: "ProctoringAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringAttempts_ResourceLinkId",
                table: "ProctoringAttempts",
                column: "ResourceLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringEvents_ProctoringAttemptId",
                table: "ProctoringEvents",
                column: "ProctoringAttemptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProctoringEvents");

            migrationBuilder.DropTable(
                name: "ProctoringAttempts");

            migrationBuilder.DropColumn(
                name: "IsProctored",
                table: "ResourceLinks");

            migrationBuilder.DropColumn(
                name: "ProctoringSettings",
                table: "ResourceLinks");
        }
    }
}
