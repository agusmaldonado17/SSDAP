using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSDAP.Api.Migrations
{
    /// <inheritdoc />
    public partial class QuitarZonaHorariaLocalidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "zona_horaria",
                table: "localidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "zona_horaria",
                table: "localidad",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "America/Argentina/Buenos_Aires");
        }
    }
}
