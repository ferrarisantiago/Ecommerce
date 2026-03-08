using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_Datos.Migrations
{
    public partial class PortfolioReadyUpgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Productos",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "OrdenDetalle",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<double>(
                name: "UnitPrice",
                table: "OrdenDetalle",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CiudadEnvio",
                table: "Orden",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostalEnvio",
                table: "Orden",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Orden",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "DireccionEnvio",
                table: "Orden",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Orden",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<string>(
                name: "PaisEnvio",
                table: "Orden",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinciaEnvio",
                table: "Orden",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Total",
                table: "Orden",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IsDeleted_CreatedAt",
                table: "Productos",
                columns: new[] { "IsDeleted", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_IsDeleted_CreatedAt",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "OrdenDetalle");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "OrdenDetalle");

            migrationBuilder.DropColumn(
                name: "CiudadEnvio",
                table: "Orden");

            migrationBuilder.DropColumn(
                name: "CodigoPostalEnvio",
                table: "Orden");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Orden");

            migrationBuilder.DropColumn(
                name: "DireccionEnvio",
                table: "Orden");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Orden");

            migrationBuilder.DropColumn(
                name: "PaisEnvio",
                table: "Orden");

            migrationBuilder.DropColumn(
                name: "ProvinciaEnvio",
                table: "Orden");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Orden");
        }
    }
}
