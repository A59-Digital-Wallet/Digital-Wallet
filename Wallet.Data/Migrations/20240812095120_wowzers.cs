using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class wowzers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWalletAssociations_AspNetUsers_AppUserId",
                table: "UserWalletAssociations");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Wallets");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "UserWalletAssociations",
                newName: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWalletAssociations_AspNetUsers_OwnerId",
                table: "UserWalletAssociations",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWalletAssociations_AspNetUsers_OwnerId",
                table: "UserWalletAssociations");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "UserWalletAssociations",
                newName: "AppUserId");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Wallets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserWalletAssociations_AspNetUsers_AppUserId",
                table: "UserWalletAssociations",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
