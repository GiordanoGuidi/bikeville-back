using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeVille.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCartTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                       migrationBuilder.CreateTable(
                name: "UserCart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderQty = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCart_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "SalesLT",
                        principalTable: "Customer",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCart_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "SalesLT",
                        principalTable: "Product",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            
            migrationBuilder.CreateIndex(
                name: "IX_UserCart_CustomerId",
                table: "UserCart",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCart_ProductId",
                table: "UserCart",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCart");
        }
    }
}
