using Ecommerce_Datos.Datos;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_Datos.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260308070000_AddProductInventoryControls")]
    public partial class AddProductInventoryControls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_IsDeleted_CreatedAt",
                table: "Productos");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumStockLevel",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Productos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IsDeleted_IsActive_CreatedAt",
                table: "Productos",
                columns: new[] { "IsDeleted", "IsActive", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_IsDeleted_IsActive_CreatedAt",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MinimumStockLevel",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Productos");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IsDeleted_CreatedAt",
                table: "Productos",
                columns: new[] { "IsDeleted", "CreatedAt" });
        }
    }
}
