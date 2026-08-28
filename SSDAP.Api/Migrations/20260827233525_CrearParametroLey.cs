using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearParametroLey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parametro_ley",
                columns: table => new
                {
                    id_parametro_ley = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nombre_parametro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    unidad = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    valor_minimo = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    valor_maximo = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    observaciones = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametro_ley", x => x.id_parametro_ley);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parametro_ley");
        }
    }
}
