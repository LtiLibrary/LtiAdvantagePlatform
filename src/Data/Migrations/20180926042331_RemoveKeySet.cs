using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class RemoveKeySet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_KeySets_KeySetId",
                table: "Deployments");

            migrationBuilder.DropTable(
                name: "KeySets");

            migrationBuilder.DropIndex(
                name: "IX_Deployments_KeySetId",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "KeySetId",
                table: "Deployments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KeySetId",
                table: "Deployments",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KeySets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PrivateKey = table.Column<string>(nullable: true),
                    PublicKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeySets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_KeySetId",
                table: "Deployments",
                column: "KeySetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_KeySets_KeySetId",
                table: "Deployments",
                column: "KeySetId",
                principalTable: "KeySets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
