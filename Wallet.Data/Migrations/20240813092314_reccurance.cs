using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class reccurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Interval",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecutedDate",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextExecutionDate",
                table: "Transactions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interval",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "LastExecutedDate",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "NextExecutionDate",
                table: "Transactions");
        }
    }
}
