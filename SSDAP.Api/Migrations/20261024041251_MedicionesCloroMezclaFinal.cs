using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class MedicionesCloroMezclaFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "medicion_cloro",
                columns: table => new
                {
                    id_medicion_cloro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    fecha_hora_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    valor_mg_l = table.Column<decimal>(type: "decimal(14,6)", precision: 14, scale: 6, nullable: false),
                    tipo_cloro = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    observaciones = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    creado_en_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    creado_por_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicion_cloro", x => x.id_medicion_cloro);
                    table.ForeignKey(
                        name: "FK_medicion_cloro_localidad_id_localidad",
                        column: x => x.id_localidad,
                        principalTable: "localidad",
                        principalColumn: "id_localidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medicion_cloro_usuario_creado_por_id",
                        column: x => x.creado_por_id,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_medicion_cloro_creado_por_id",
                table: "medicion_cloro",
                column: "creado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_medicion_cloro_id_localidad_fecha_hora_utc_id_medicion_cloro",
                table: "medicion_cloro",
                columns: new[] { "id_localidad", "fecha_hora_utc", "id_medicion_cloro" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medicion_cloro");
        }
    }
}
