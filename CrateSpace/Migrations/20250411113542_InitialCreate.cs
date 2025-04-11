using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CrateSpace.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cratespace");

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                schema: "cratespace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Name of the inventory item"),
                    Quantity = table.Column<int>(type: "integer", nullable: false, comment: "Current quantity in stock"),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Price of the item"),
                    LastRestocked = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Last restock date and time"),
                    MinimumQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 10, comment: "Minimum quantity threshold for reordering")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                },
                comment: "Stores inventory item information");

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "cratespace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Name of the ordered item"),
                    Quantity = table.Column<int>(type: "integer", nullable: false, comment: "Quantity of items ordered"),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Date and time when the order was created"),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Total price of the order"),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending", comment: "Current status of the order")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                },
                comment: "Stores all order information");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Name",
                schema: "cratespace",
                table: "InventoryItems",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Quantity",
                schema: "cratespace",
                table: "InventoryItems",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ItemName",
                schema: "cratespace",
                table: "Orders",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                schema: "cratespace",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                schema: "cratespace",
                table: "Orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryItems",
                schema: "cratespace");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "cratespace");
        }
    }
}
