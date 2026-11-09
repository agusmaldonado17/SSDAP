using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class EdicionAnalisisConVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "modificado_en_utc",
                table: "analisis_calidad",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "modificado_por_id",
                table: "analisis_calidad",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "analisis_calidad",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_analisis_calidad_modificado_por_id",
                table: "analisis_calidad",
                column: "modificado_por_id");

            migrationBuilder.AddForeignKey(
                name: "FK_analisis_calidad_usuario_modificado_por_id",
                table: "analisis_calidad",
                column: "modificado_por_id",
                principalTable: "usuario",
                principalColumn: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_analisis_calidad_usuario_modificado_por_id",
                table: "analisis_calidad");

            migrationBuilder.DropIndex(
                name: "IX_analisis_calidad_modificado_por_id",
                table: "analisis_calidad");

            migrationBuilder.DropColumn(
                name: "modificado_en_utc",
                table: "analisis_calidad");

            migrationBuilder.DropColumn(
                name: "modificado_por_id",
                table: "analisis_calidad");

            migrationBuilder.DropColumn(
                name: "version",
                table: "analisis_calidad");
        }
    }
}
