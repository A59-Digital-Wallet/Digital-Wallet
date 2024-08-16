using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class OverdraftSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OverdraftSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultInterestRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 0.05m),
                    DefaultOverdraftLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 500m),
                    DefaultConsecutiveNegativeMonths = table.Column<int>(type: "int", nullable: false, defaultValue: 3)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdraftSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftSettings_Id",
                table: "OverdraftSettings",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OverdraftSettings");
        }
    }
}
