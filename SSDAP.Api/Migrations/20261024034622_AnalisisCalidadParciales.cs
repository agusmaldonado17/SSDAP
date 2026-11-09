using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class AnalisisCalidadParciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_analisis_calidad_fuente_agua_id_fuente",
                table: "analisis_calidad");

            migrationBuilder.DropIndex(
                name: "IX_analisis_calidad_id_fuente",
                table: "analisis_calidad");

            migrationBuilder.AlterColumn<decimal>(
                name: "sulfato",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "sodio",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "residuo_seco",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ras",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "potasio",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ph",
                table: "analisis_calidad",
                type: "decimal(4,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "nitrito",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "nitrato",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "magnesio",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "hierro",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "fluor",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "dureza_total",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "conductividad",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "cloruro",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "carbonato",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "calcio",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "bicarbonato",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "arsenico",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "amonio",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "alcalinidad_total",
                table: "analisis_calidad",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.CreateIndex(
                name: "IX_analisis_calidad_id_fuente_fecha_id_analisis",
                table: "analisis_calidad",
                columns: new[] { "id_fuente", "fecha", "id_analisis" });

            migrationBuilder.AddForeignKey(
                name: "FK_analisis_calidad_fuente_agua_id_fuente",
                table: "analisis_calidad",
                column: "id_fuente",
                principalTable: "fuente_agua",
                principalColumn: "id_fuente",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_analisis_calidad_fuente_agua_id_fuente",
                table: "analisis_calidad");

            migrationBuilder.DropIndex(
                name: "IX_analisis_calidad_id_fuente_fecha_id_analisis",
                table: "analisis_calidad");

            migrationBuilder.AlterColumn<decimal>(
                name: "sulfato",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "sodio",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "residuo_seco",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ras",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "potasio",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ph",
                table: "analisis_calidad",
                type: "decimal(4,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "nitrito",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "nitrato",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "magnesio",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "hierro",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "fluor",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "dureza_total",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "conductividad",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "cloruro",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "carbonato",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "calcio",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "bicarbonato",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "arsenico",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amonio",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "alcalinidad_total",
                table: "analisis_calidad",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_analisis_calidad_id_fuente",
                table: "analisis_calidad",
                column: "id_fuente");

            migrationBuilder.AddForeignKey(
                name: "FK_analisis_calidad_fuente_agua_id_fuente",
                table: "analisis_calidad",
                column: "id_fuente",
                principalTable: "fuente_agua",
                principalColumn: "id_fuente",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
