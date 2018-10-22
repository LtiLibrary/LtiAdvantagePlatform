using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class RemoveMyClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_MyClients_MyClientId",
                table: "Deployments");

            migrationBuilder.DropTable(
                name: "MyClients");

            migrationBuilder.DropIndex(
                name: "IX_Deployments_MyClientId",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "MyClientId",
                table: "Deployments");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Deployments",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Deployments");

            migrationBuilder.AddColumn<string>(
                name: "MyClientId",
                table: "Deployments",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MyClients",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyClients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_MyClientId",
                table: "Deployments",
                column: "MyClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_MyClients_MyClientId",
                table: "Deployments",
                column: "MyClientId",
                principalTable: "MyClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
