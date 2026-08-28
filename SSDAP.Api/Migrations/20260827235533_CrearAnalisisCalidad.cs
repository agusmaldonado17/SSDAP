using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearAnalisisCalidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analisis_calidad",
                columns: table => new
                {
                    id_analisis = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_fuente = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    residuo_seco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    conductividad = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ph = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    cloruro = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    sulfato = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    alcalinidad_total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    carbonato = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    bicarbonato = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    dureza_total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    calcio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    magnesio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    hierro = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    nitrato = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    nitrito = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    amonio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fluor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    arsenico = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    sodio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    potasio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ras = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    observaciones = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analisis_calidad", x => x.id_analisis);
                    table.ForeignKey(
                        name: "FK_analisis_calidad_fuente_agua_id_fuente",
                        column: x => x.id_fuente,
                        principalTable: "fuente_agua",
                        principalColumn: "id_fuente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_analisis_calidad_id_fuente",
                table: "analisis_calidad",
                column: "id_fuente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analisis_calidad");
        }
    }
}
