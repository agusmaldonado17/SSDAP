using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearBateriaPozo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bateria_pozo",
                columns: table => new
                {
                    id_bateria_pozo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_bateria = table.Column<int>(type: "int", nullable: false),
                    id_pozo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bateria_pozo", x => x.id_bateria_pozo);
                    table.ForeignKey(
                        name: "FK_bateria_pozo_bateria_id_bateria",
                        column: x => x.id_bateria,
                        principalTable: "bateria",
                        principalColumn: "id_bateria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bateria_pozo_pozo_id_pozo",
                        column: x => x.id_pozo,
                        principalTable: "pozo",
                        principalColumn: "id_pozo",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_bateria_pozo_id_bateria",
                table: "bateria_pozo",
                column: "id_bateria");

            migrationBuilder.CreateIndex(
                name: "IX_bateria_pozo_id_pozo",
                table: "bateria_pozo",
                column: "id_pozo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bateria_pozo");
        }
    }
}
