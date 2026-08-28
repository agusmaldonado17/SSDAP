using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearPozo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pozo",
                columns: table => new
                {
                    id_pozo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_fuente = table.Column<int>(type: "int", nullable: false),
                    codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    latitud = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    longitud = table.Column<decimal>(type: "decimal(10,7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pozo", x => x.id_pozo);
                    table.ForeignKey(
                        name: "FK_pozo_fuente_agua_id_fuente",
                        column: x => x.id_fuente,
                        principalTable: "fuente_agua",
                        principalColumn: "id_fuente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_pozo_id_fuente",
                table: "pozo",
                column: "id_fuente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pozo");
        }
    }
}
