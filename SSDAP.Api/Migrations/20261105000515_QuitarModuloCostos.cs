using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class QuitarModuloCostos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Retira únicamente el bloque económico de las capturas existentes.
            // Se conservan versión, revisión, producción, calidad y metadatos históricos.
            migrationBuilder.Sql("""
                UPDATE resultado_mensual
                SET reporte_json = JSON_REMOVE(reporte_json, '$.costos')
                WHERE reporte_json IS NOT NULL AND JSON_VALID(reporte_json)
                  AND version_formato IN (2, 3)
                  AND JSON_CONTAINS_PATH(reporte_json, 'one', '$.costos');
                """);

            migrationBuilder.DropTable(
                name: "amortizacion_costo_mensual");

            migrationBuilder.DropTable(
                name: "personal_costo_mensual");

            migrationBuilder.DropTable(
                name: "potencia_pozo_mensual");

            migrationBuilder.DropTable(
                name: "precio_acueducto_mensual");

            migrationBuilder.DropTable(
                name: "costos_complementarios");

            migrationBuilder.DropTable(
                name: "costos_mensuales");

            migrationBuilder.DropColumn(
                name: "potencia_kw",
                table: "bateria");

            migrationBuilder.DropColumn(
                name: "costo_m3",
                table: "acueducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaura la estructura, pero no recupera importes ni capturas económicas eliminados.
            migrationBuilder.AddColumn<decimal>(
                name: "potencia_kw",
                table: "bateria",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "costo_m3",
                table: "acueducto",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "costos_complementarios",
                columns: table => new
                {
                    id_costos = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    actualizado_en_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_por_id = table.Column<int>(type: "int", nullable: false),
                    amortizaciones_informadas = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    cantidad_cloracion = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    gasto_cloracion = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    mantenimiento_mensual = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    mes = table.Column<int>(type: "int", nullable: false),
                    modo_cloracion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    personal_informado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    precio_cloracion = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    unidad_cloracion = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_costos_complementarios", x => x.id_costos);
                    table.ForeignKey(
                        name: "FK_costos_complementarios_localidad_id_localidad",
                        column: x => x.id_localidad,
                        principalTable: "localidad",
                        principalColumn: "id_localidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_costos_complementarios_usuario_actualizado_por_id",
                        column: x => x.actualizado_por_id,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "costos_mensuales",
                columns: table => new
                {
                    id_costos_mensuales = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    actualizado_en_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_por_id = table.Column<int>(type: "int", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    mes = table.Column<int>(type: "int", nullable: false),
                    moneda = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    precio_kwh = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_costos_mensuales", x => x.id_costos_mensuales);
                    table.ForeignKey(
                        name: "FK_costos_mensuales_localidad_id_localidad",
                        column: x => x.id_localidad,
                        principalTable: "localidad",
                        principalColumn: "id_localidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_costos_mensuales_usuario_actualizado_por_id",
                        column: x => x.actualizado_por_id,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "amortizacion_costo_mensual",
                columns: table => new
                {
                    id_costos = table.Column<int>(type: "int", nullable: false),
                    orden = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    valor_residual_unitario = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false),
                    vida_util_meses = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_amortizacion_costo_mensual", x => new { x.id_costos, x.orden });
                    table.ForeignKey(
                        name: "FK_amortizacion_costo_mensual_costos_complementarios_id_costos",
                        column: x => x.id_costos,
                        principalTable: "costos_complementarios",
                        principalColumn: "id_costos",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "personal_costo_mensual",
                columns: table => new
                {
                    id_costos = table.Column<int>(type: "int", nullable: false),
                    orden = table.Column<int>(type: "int", nullable: false),
                    funcion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    importe_mensual = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false),
                    porcentaje_afectacion = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personal_costo_mensual", x => new { x.id_costos, x.orden });
                    table.ForeignKey(
                        name: "FK_personal_costo_mensual_costos_complementarios_id_costos",
                        column: x => x.id_costos,
                        principalTable: "costos_complementarios",
                        principalColumn: "id_costos",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "potencia_pozo_mensual",
                columns: table => new
                {
                    id_costos_mensuales = table.Column<int>(type: "int", nullable: false),
                    id_pozo = table.Column<int>(type: "int", nullable: false),
                    potencia_kw = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_potencia_pozo_mensual", x => new { x.id_costos_mensuales, x.id_pozo });
                    table.ForeignKey(
                        name: "FK_potencia_pozo_mensual_costos_mensuales_id_costos_mensuales",
                        column: x => x.id_costos_mensuales,
                        principalTable: "costos_mensuales",
                        principalColumn: "id_costos_mensuales",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_potencia_pozo_mensual_pozo_id_pozo",
                        column: x => x.id_pozo,
                        principalTable: "pozo",
                        principalColumn: "id_pozo",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "precio_acueducto_mensual",
                columns: table => new
                {
                    id_costos_mensuales = table.Column<int>(type: "int", nullable: false),
                    id_fuente = table.Column<int>(type: "int", nullable: false),
                    precio_m3 = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precio_acueducto_mensual", x => new { x.id_costos_mensuales, x.id_fuente });
                    table.ForeignKey(
                        name: "FK_precio_acueducto_mensual_costos_mensuales_id_costos_mensuales",
                        column: x => x.id_costos_mensuales,
                        principalTable: "costos_mensuales",
                        principalColumn: "id_costos_mensuales",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_precio_acueducto_mensual_fuente_agua_id_fuente",
                        column: x => x.id_fuente,
                        principalTable: "fuente_agua",
                        principalColumn: "id_fuente",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_costos_complementarios_actualizado_por_id",
                table: "costos_complementarios",
                column: "actualizado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_costos_complementarios_id_localidad_anio_mes",
                table: "costos_complementarios",
                columns: new[] { "id_localidad", "anio", "mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_costos_mensuales_actualizado_por_id",
                table: "costos_mensuales",
                column: "actualizado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_costos_mensuales_id_localidad_anio_mes",
                table: "costos_mensuales",
                columns: new[] { "id_localidad", "anio", "mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_potencia_pozo_mensual_id_pozo",
                table: "potencia_pozo_mensual",
                column: "id_pozo");

            migrationBuilder.CreateIndex(
                name: "IX_precio_acueducto_mensual_id_fuente",
                table: "precio_acueducto_mensual",
                column: "id_fuente");
        }
    }
}
