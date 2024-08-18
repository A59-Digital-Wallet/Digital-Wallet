using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OverdraftSettings",
                columns: new[] { "Id", "DefaultConsecutiveNegativeMonths", "DefaultInterestRate", "DefaultOverdraftLimit" },
                values: new object[] { 1, 3, 0.05m, 500m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OverdraftSettings",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
