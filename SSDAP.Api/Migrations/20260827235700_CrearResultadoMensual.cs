using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearResultadoMensual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resultado_mensual",
                columns: table => new
                {
                    id_resultado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    mes = table.Column<int>(type: "int", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    caudal_total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    dosis_cloro = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    estado_calidad = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    fecha_generacion = table.Column<DateOnly>(type: "date", nullable: false),
                    observaciones = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultado_mensual", x => x.id_resultado);
                    table.ForeignKey(
                        name: "FK_resultado_mensual_localidad_id_localidad",
                        column: x => x.id_localidad,
                        principalTable: "localidad",
                        principalColumn: "id_localidad",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_resultado_mensual_id_localidad",
                table: "resultado_mensual",
                column: "id_localidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resultado_mensual");
        }
    }
}
