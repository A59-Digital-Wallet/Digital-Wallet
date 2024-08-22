using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class newWalletThingy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastSelectedWalletId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastSelectedWalletId",
                table: "AspNetUsers",
                column: "LastSelectedWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Wallets_LastSelectedWalletId",
                table: "AspNetUsers",
                column: "LastSelectedWalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Wallets_LastSelectedWalletId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LastSelectedWalletId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastSelectedWalletId",
                table: "AspNetUsers");
        }
    }
}
