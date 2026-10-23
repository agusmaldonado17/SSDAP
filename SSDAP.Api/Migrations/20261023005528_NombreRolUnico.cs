using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class NombreRolUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El proveedor MySQL no emite la collation de AlterColumn en el SQL generado.
            migrationBuilder.Sql(
                "ALTER TABLE `rol` MODIFY `nombre` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_rol_nombre",
                table: "rol",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rol_nombre",
                table: "rol");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "rol",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldCollation: "utf8mb4_unicode_ci");
        }
    }
}
