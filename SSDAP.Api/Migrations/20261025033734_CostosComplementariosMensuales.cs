using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CostosComplementariosMensuales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "costos_complementarios",
                columns: table => new
                {
                    id_costos = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    mes = table.Column<int>(type: "int", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    modo_cloracion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    gasto_cloracion = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    cantidad_cloracion = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    unidad_cloracion = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    precio_cloracion = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    personal_informado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    amortizaciones_informadas = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    mantenimiento_mensual = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    actualizado_por_id = table.Column<int>(type: "int", nullable: false),
                    actualizado_en_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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
                name: "amortizacion_costo_mensual",
                columns: table => new
                {
                    id_costos = table.Column<int>(type: "int", nullable: false),
                    orden = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false),
                    valor_residual_unitario = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_costos_complementarios_actualizado_por_id",
                table: "costos_complementarios",
                column: "actualizado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_costos_complementarios_id_localidad_anio_mes",
                table: "costos_complementarios",
                columns: new[] { "id_localidad", "anio", "mes" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "amortizacion_costo_mensual");

            migrationBuilder.DropTable(
                name: "personal_costo_mensual");

            migrationBuilder.DropTable(
                name: "costos_complementarios");
        }
    }
}
