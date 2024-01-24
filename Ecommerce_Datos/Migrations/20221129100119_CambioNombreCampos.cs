using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_Datos.Migrations
{
    public partial class CambioNombreCampos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrecioPorMetroCuadrado",
                table: "ventaDetalles",
                newName: "Precio");

            migrationBuilder.RenameColumn(
                name: "MetroCuadrado",
                table: "ventaDetalles",
                newName: "Cantidad");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "ventaDetalles",
                newName: "PrecioPorMetroCuadrado");

            migrationBuilder.RenameColumn(
                name: "Cantidad",
                table: "ventaDetalles",
                newName: "MetroCuadrado");
        }
    }
}
