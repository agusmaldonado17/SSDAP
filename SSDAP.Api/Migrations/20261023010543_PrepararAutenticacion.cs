using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class PrepararAutenticacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "password",
                table: "usuario",
                newName: "password_hash");

            migrationBuilder.AddColumn<string>(
                name: "email_normalizado",
                table: "usuario",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "rol",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.Sql("UPDATE `usuario` SET `email_normalizado` = UPPER(TRIM(`email`));");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_email_normalizado",
                table: "usuario",
                column: "email_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rol_codigo",
                table: "rol",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuario_email_normalizado",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "IX_rol_codigo",
                table: "rol");

            migrationBuilder.DropColumn(
                name: "email_normalizado",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "rol");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "usuario",
                newName: "password");
        }
    }
}
