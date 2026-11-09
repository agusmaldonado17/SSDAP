using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CostosEnergiaYAguaMensuales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "costos_mensuales",
                columns: table => new
                {
                    id_costos_mensuales = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    mes = table.Column<int>(type: "int", nullable: false),
                    moneda = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    precio_kwh = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    actualizado_por_id = table.Column<int>(type: "int", nullable: false),
                    actualizado_en_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "potencia_pozo_mensual");

            migrationBuilder.DropTable(
                name: "precio_acueducto_mensual");

            migrationBuilder.DropTable(
                name: "costos_mensuales");
        }
    }
}
