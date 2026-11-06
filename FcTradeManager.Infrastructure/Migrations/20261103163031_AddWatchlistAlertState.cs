using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FcTradeManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchlistAlertState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BuyAlertTriggered",
                table: "WatchlistItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SellAlertTriggered",
                table: "WatchlistItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyAlertTriggered",
                table: "WatchlistItems");

            migrationBuilder.DropColumn(
                name: "SellAlertTriggered",
                table: "WatchlistItems");
        }
    }
}
