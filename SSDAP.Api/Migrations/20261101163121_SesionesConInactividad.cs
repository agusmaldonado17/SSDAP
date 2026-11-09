using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class SesionesConInactividad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sesion_usuario",
                columns: table => new
                {
                    id_sesion = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    creada_en_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ultima_actividad_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expira_en_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion_usuario", x => x.id_sesion);
                    table.ForeignKey(
                        name: "FK_sesion_usuario_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_sesion_usuario_expira_en_utc",
                table: "sesion_usuario",
                column: "expira_en_utc");

            migrationBuilder.CreateIndex(
                name: "IX_sesion_usuario_id_usuario",
                table: "sesion_usuario",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sesion_usuario");
        }
    }
}
