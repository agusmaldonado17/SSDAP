using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class AnalisisCalidadConfiguration : IEntityTypeConfiguration<AnalisisCalidad>
{
    public void Configure(EntityTypeBuilder<AnalisisCalidad> builder)
    {
        builder.Property(a => a.Fecha)
            .HasConversion(fecha => fecha.ToDateTime(TimeOnly.MinValue), fecha => DateOnly.FromDateTime(fecha))
            .HasColumnType("date");
        builder.HasIndex(a => new { a.IdFuente, a.Fecha, a.IdAnalisis });
        builder.HasOne(a => a.FuenteAgua).WithMany().HasForeignKey(a => a.IdFuente)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
