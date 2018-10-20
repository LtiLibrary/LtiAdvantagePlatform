using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class RenameClients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_Clients_ClientId",
                table: "Deployments");

            migrationBuilder.DropForeignKey(
                name: "FK_Platforms_KeyPairs_KeyPairId",
                table: "Platforms");

            migrationBuilder.DropTable(
                name: "KeyPairs");

            migrationBuilder.DropIndex(
                name: "IX_Platforms_KeyPairId",
                table: "Platforms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "KeyPairId",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "PrivateKey",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "Clients");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "MyClients");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Platforms",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_MyClients",
                table: "MyClients",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_MyClients_ClientId",
                table: "Deployments",
                column: "ClientId",
                principalTable: "MyClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployments_MyClients_ClientId",
                table: "Deployments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MyClients",
                table: "MyClients");

            migrationBuilder.RenameTable(
                name: "MyClients",
                newName: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Platforms",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyPairId",
                table: "Platforms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateKey",
                table: "Clients",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "Clients",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients",
                table: "Clients",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "KeyPairs",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    PrivateKey = table.Column<string>(nullable: true),
                    PublicKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyPairs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_KeyPairId",
                table: "Platforms",
                column: "KeyPairId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployments_Clients_ClientId",
                table: "Deployments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Platforms_KeyPairs_KeyPairId",
                table: "Platforms",
                column: "KeyPairId",
                principalTable: "KeyPairs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
