using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class RenameClientToMyClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_MyClients_ClientId",
                table: "Deployments");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Deployments",
                newName: "MyClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Deployments_ClientId",
                table: "Deployments",
                newName: "IX_Deployments_MyClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_MyClients_MyClientId",
                table: "Deployments",
                column: "MyClientId",
                principalTable: "MyClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_MyClients_MyClientId",
                table: "Deployments");

            migrationBuilder.RenameColumn(
                name: "MyClientId",
                table: "Deployments",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Deployments_MyClientId",
                table: "Deployments",
                newName: "IX_Deployments_ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_MyClients_ClientId",
                table: "Deployments",
                column: "ClientId",
                principalTable: "MyClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
