using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class ChangeDeploymentIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Deployments",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Deployments");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Deployments",
                nullable: false)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deployments",
                table: "Deployments",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Deployments",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Deployments");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Deployments",
                nullable: false)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deployments",
                table: "Deployments",
                column: "Id");
        }
    }
}
