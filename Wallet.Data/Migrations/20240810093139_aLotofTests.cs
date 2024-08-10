using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class aLotofTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_RecipientWalletId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserCardID",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_RecipientWalletId",
                table: "Transactions",
                column: "RecipientWalletId",
                principalTable: "Wallets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_RecipientWalletId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "UserCardID",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_RecipientWalletId",
                table: "Transactions",
                column: "RecipientWalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
