using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class ParametrosCalidadConfigurables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "valor_minimo",
                table: "parametro_ley",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "valor_maximo",
                table: "parametro_ley",
                type: "decimal(14,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "activo",
                table: "parametro_ley",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "parametro_ley",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true,
                collation: "utf8mb4_bin");

            migrationBuilder.AddColumn<string>(
                name: "referencia",
                table: "parametro_ley",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "parametro_ley",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_parametro_ley_codigo",
                table: "parametro_ley",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_parametro_ley_codigo",
                table: "parametro_ley");

            migrationBuilder.DropColumn(
                name: "activo",
                table: "parametro_ley");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "parametro_ley");

            migrationBuilder.DropColumn(
                name: "referencia",
                table: "parametro_ley");

            migrationBuilder.DropColumn(
                name: "version",
                table: "parametro_ley");

            migrationBuilder.AlterColumn<decimal>(
                name: "valor_minimo",
                table: "parametro_ley",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "valor_maximo",
                table: "parametro_ley",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,6)",
                oldNullable: true);
        }
    }
}
