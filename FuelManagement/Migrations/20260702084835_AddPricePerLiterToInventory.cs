using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuelManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPricePerLiterToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerLiter",
                table: "Inventories",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerLiter",
                table: "Inventories");
        }
    }
}
