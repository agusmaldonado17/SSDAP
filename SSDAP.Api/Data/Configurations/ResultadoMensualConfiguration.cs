using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class ResultadoMensualConfiguration : IEntityTypeConfiguration<ResultadoMensual>
{
    public void Configure(EntityTypeBuilder<ResultadoMensual> builder)
    {
        builder.Property(r => r.FechaGeneracion)
            .HasConversion(fecha => fecha.ToDateTime(TimeOnly.MinValue), fecha => DateOnly.FromDateTime(fecha))
            .HasColumnType("date");
        builder.HasIndex(r => new { r.IdLocalidad, r.Anio, r.Mes, r.Revision }).IsUnique();
        builder.HasOne(r => r.Localidad).WithMany().HasForeignKey(r => r.IdLocalidad).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(r => r.CreadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}
