using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearFuenteAgua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fuente_agua",
                columns: table => new
                {
                    id_fuente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    tipo_fuente = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    caudal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    estado = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    observaciones = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fuente_agua", x => x.id_fuente);
                    table.ForeignKey(
                        name: "FK_fuente_agua_localidad_id_localidad",
                        column: x => x.id_localidad,
                        principalTable: "localidad",
                        principalColumn: "id_localidad",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_fuente_agua_id_localidad",
                table: "fuente_agua",
                column: "id_localidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fuente_agua");
        }
    }
}
