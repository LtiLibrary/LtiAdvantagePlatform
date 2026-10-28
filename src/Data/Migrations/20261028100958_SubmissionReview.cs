using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvantagePlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubmissionReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSubmissionReview",
                table: "GradebookColumns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionReviewCustom",
                table: "GradebookColumns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionReviewLabel",
                table: "GradebookColumns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionReviewUrl",
                table: "GradebookColumns",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasSubmissionReview",
                table: "GradebookColumns");

            migrationBuilder.DropColumn(
                name: "SubmissionReviewCustom",
                table: "GradebookColumns");

            migrationBuilder.DropColumn(
                name: "SubmissionReviewLabel",
                table: "GradebookColumns");

            migrationBuilder.DropColumn(
                name: "SubmissionReviewUrl",
                table: "GradebookColumns");
        }
    }
}
