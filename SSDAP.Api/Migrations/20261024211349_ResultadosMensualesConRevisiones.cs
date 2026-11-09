using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class ResultadosMensualesConRevisiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_resultado_mensual_localidad_id_localidad",
                table: "resultado_mensual");

            migrationBuilder.DropIndex(
                name: "IX_resultado_mensual_id_localidad",
                table: "resultado_mensual");

            migrationBuilder.AlterColumn<decimal>(
                name: "dosis_cloro",
                table: "resultado_mensual",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "caudal_total",
                table: "resultado_mensual",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "creado_en_utc",
                table: "resultado_mensual",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "creado_por_id",
                table: "resultado_mensual",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reporte_json",
                table: "resultado_mensual",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "revision",
                table: "resultado_mensual",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version_formato",
                table: "resultado_mensual",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_resultado_mensual_creado_por_id",
                table: "resultado_mensual",
                column: "creado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_resultado_mensual_id_localidad_anio_mes_revision",
                table: "resultado_mensual",
                columns: new[] { "id_localidad", "anio", "mes", "revision" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_resultado_mensual_localidad_id_localidad",
                table: "resultado_mensual",
                column: "id_localidad",
                principalTable: "localidad",
                principalColumn: "id_localidad",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_resultado_mensual_usuario_creado_por_id",
                table: "resultado_mensual",
                column: "creado_por_id",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_resultado_mensual_localidad_id_localidad",
                table: "resultado_mensual");

            migrationBuilder.DropForeignKey(
                name: "FK_resultado_mensual_usuario_creado_por_id",
                table: "resultado_mensual");

            migrationBuilder.DropIndex(
                name: "IX_resultado_mensual_creado_por_id",
                table: "resultado_mensual");

            migrationBuilder.DropIndex(
                name: "IX_resultado_mensual_id_localidad_anio_mes_revision",
                table: "resultado_mensual");

            migrationBuilder.DropColumn(
                name: "creado_en_utc",
                table: "resultado_mensual");

            migrationBuilder.DropColumn(
                name: "creado_por_id",
                table: "resultado_mensual");

            migrationBuilder.DropColumn(
                name: "reporte_json",
                table: "resultado_mensual");

            migrationBuilder.DropColumn(
                name: "revision",
                table: "resultado_mensual");

            migrationBuilder.DropColumn(
                name: "version_formato",
                table: "resultado_mensual");

            migrationBuilder.AlterColumn<decimal>(
                name: "dosis_cloro",
                table: "resultado_mensual",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "caudal_total",
                table: "resultado_mensual",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_resultado_mensual_id_localidad",
                table: "resultado_mensual",
                column: "id_localidad");

            migrationBuilder.AddForeignKey(
                name: "FK_resultado_mensual_localidad_id_localidad",
                table: "resultado_mensual",
                column: "id_localidad",
                principalTable: "localidad",
                principalColumn: "id_localidad",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
