using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvantagePlatform.Migrations
{
    public partial class AddKeyPairs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateKey",
                table: "Platforms");

            migrationBuilder.RenameColumn(
                name: "PublicKey",
                table: "Platforms",
                newName: "KeyPairId");

            migrationBuilder.AlterColumn<string>(
                name: "KeyPairId",
                table: "Platforms",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

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
                name: "FK_Platforms_KeyPairs_KeyPairId",
                table: "Platforms",
                column: "KeyPairId",
                principalTable: "KeyPairs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Platforms_KeyPairs_KeyPairId",
                table: "Platforms");

            migrationBuilder.DropTable(
                name: "KeyPairs");

            migrationBuilder.DropIndex(
                name: "IX_Platforms_KeyPairId",
                table: "Platforms");

            migrationBuilder.RenameColumn(
                name: "KeyPairId",
                table: "Platforms",
                newName: "PublicKey");

            migrationBuilder.AlterColumn<string>(
                name: "PublicKey",
                table: "Platforms",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateKey",
                table: "Platforms",
                nullable: true);
        }
    }
}
