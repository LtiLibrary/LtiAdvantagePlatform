using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Data.Migrations
{
    public partial class AddClientToDeployment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Deployments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_ClientId",
                table: "Deployments",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_Clients_ClientId",
                table: "Deployments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_Clients_ClientId",
                table: "Deployments");

            migrationBuilder.DropIndex(
                name: "IX_Deployments_ClientId",
                table: "Deployments");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Deployments");
        }
    }
}
