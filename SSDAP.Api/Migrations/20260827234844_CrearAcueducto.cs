using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearAcueducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acueducto",
                columns: table => new
                {
                    id_acueducto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    id_fuente = table.Column<int>(type: "int", nullable: false),
                    costo_m3 = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acueducto", x => x.id_acueducto);
                    table.ForeignKey(
                        name: "FK_acueducto_fuente_agua_id_fuente",
                        column: x => x.id_fuente,
                        principalTable: "fuente_agua",
                        principalColumn: "id_fuente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_acueducto_id_fuente",
                table: "acueducto",
                column: "id_fuente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acueducto");
        }
    }
}
