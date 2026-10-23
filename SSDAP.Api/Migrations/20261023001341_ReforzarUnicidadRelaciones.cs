using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    public partial class ReforzarUnicidadRelaciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear el reemplazo antes de quitar el índice que sostiene la clave foránea.
            migrationBuilder.CreateIndex(
                name: "IX_usuario_localidad_id_usuario_id_localidad",
                table: "usuario_localidad",
                columns: new[] { "id_usuario", "id_localidad" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_usuario_localidad_id_usuario",
                table: "usuario_localidad");

            migrationBuilder.CreateIndex(
                name: "IX_pozo_id_fuente_unico",
                table: "pozo",
                column: "id_fuente",
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_pozo_id_fuente",
                table: "pozo");

            migrationBuilder.CreateIndex(
                name: "IX_bateria_pozo_id_bateria_id_pozo",
                table: "bateria_pozo",
                columns: new[] { "id_bateria", "id_pozo" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_bateria_pozo_id_bateria",
                table: "bateria_pozo");

            migrationBuilder.CreateIndex(
                name: "IX_acueducto_id_fuente_unico",
                table: "acueducto",
                column: "id_fuente",
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_acueducto_id_fuente",
                table: "acueducto");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Crear el reemplazo antes de quitar el índice que sostiene la clave foránea.
            migrationBuilder.CreateIndex(
                name: "IX_usuario_localidad_id_usuario",
                table: "usuario_localidad",
                column: "id_usuario");

            migrationBuilder.DropIndex(
                name: "IX_usuario_localidad_id_usuario_id_localidad",
                table: "usuario_localidad");

            migrationBuilder.CreateIndex(
                name: "IX_pozo_id_fuente",
                table: "pozo",
                column: "id_fuente");

            migrationBuilder.DropIndex(
                name: "IX_pozo_id_fuente_unico",
                table: "pozo");

            migrationBuilder.CreateIndex(
                name: "IX_bateria_pozo_id_bateria",
                table: "bateria_pozo",
                column: "id_bateria");

            migrationBuilder.DropIndex(
                name: "IX_bateria_pozo_id_bateria_id_pozo",
                table: "bateria_pozo");

            migrationBuilder.CreateIndex(
                name: "IX_acueducto_id_fuente",
                table: "acueducto",
                column: "id_fuente");

            migrationBuilder.DropIndex(
                name: "IX_acueducto_id_fuente_unico",
                table: "acueducto");

        }
    }
}
