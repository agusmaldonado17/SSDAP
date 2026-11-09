using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class CalendarioConDetallePozos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_programacion_bateria_bateria_id_bateria",
                table: "programacion_bateria");

            migrationBuilder.AddColumn<DateTime>(
                name: "creado_en_utc",
                table: "programacion_bateria",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "creado_por_id",
                table: "programacion_bateria",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "programacion_bateria",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "PendienteRevision");

            migrationBuilder.AddColumn<DateTime>(
                name: "fin_utc",
                table: "programacion_bateria",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "inicio_utc",
                table: "programacion_bateria",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modificado_en_utc",
                table: "programacion_bateria",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "modificado_por_id",
                table: "programacion_bateria",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre_bateria",
                table: "programacion_bateria",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "programacion_bateria",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "zona_horaria",
                table: "localidad",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "America/Argentina/Buenos_Aires");

            migrationBuilder.CreateTable(
                name: "programacion_pozo",
                columns: table => new
                {
                    id_programacion = table.Column<int>(type: "int", nullable: false),
                    id_pozo = table.Column<int>(type: "int", nullable: false),
                    codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    caudal_m3_h = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programacion_pozo", x => new { x.id_programacion, x.id_pozo });
                    table.ForeignKey(
                        name: "FK_programacion_pozo_pozo_id_pozo",
                        column: x => x.id_pozo,
                        principalTable: "pozo",
                        principalColumn: "id_pozo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programacion_pozo_programacion_bateria_id_programacion",
                        column: x => x.id_programacion,
                        principalTable: "programacion_bateria",
                        principalColumn: "id_programacion",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_programacion_bateria_creado_por_id",
                table: "programacion_bateria",
                column: "creado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_programacion_bateria_inicio_utc_fin_utc",
                table: "programacion_bateria",
                columns: new[] { "inicio_utc", "fin_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_programacion_bateria_modificado_por_id",
                table: "programacion_bateria",
                column: "modificado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_programacion_pozo_id_pozo",
                table: "programacion_pozo",
                column: "id_pozo");

            migrationBuilder.AddForeignKey(
                name: "FK_programacion_bateria_bateria_id_bateria",
                table: "programacion_bateria",
                column: "id_bateria",
                principalTable: "bateria",
                principalColumn: "id_bateria",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_programacion_bateria_usuario_creado_por_id",
                table: "programacion_bateria",
                column: "creado_por_id",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_programacion_bateria_usuario_modificado_por_id",
                table: "programacion_bateria",
                column: "modificado_por_id",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_programacion_bateria_bateria_id_bateria",
                table: "programacion_bateria");

            migrationBuilder.DropForeignKey(
                name: "FK_programacion_bateria_usuario_creado_por_id",
                table: "programacion_bateria");

            migrationBuilder.DropForeignKey(
                name: "FK_programacion_bateria_usuario_modificado_por_id",
                table: "programacion_bateria");

            migrationBuilder.DropTable(
                name: "programacion_pozo");

            migrationBuilder.DropIndex(
                name: "IX_programacion_bateria_creado_por_id",
                table: "programacion_bateria");

            migrationBuilder.DropIndex(
                name: "IX_programacion_bateria_inicio_utc_fin_utc",
                table: "programacion_bateria");

            migrationBuilder.DropIndex(
                name: "IX_programacion_bateria_modificado_por_id",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "creado_en_utc",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "creado_por_id",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "fin_utc",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "inicio_utc",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "modificado_en_utc",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "modificado_por_id",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "nombre_bateria",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "version",
                table: "programacion_bateria");

            migrationBuilder.DropColumn(
                name: "zona_horaria",
                table: "localidad");

            migrationBuilder.AddForeignKey(
                name: "FK_programacion_bateria_bateria_id_bateria",
                table: "programacion_bateria",
                column: "id_bateria",
                principalTable: "bateria",
                principalColumn: "id_bateria",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
