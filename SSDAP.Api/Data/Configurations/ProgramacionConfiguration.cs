using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class ProgramacionConfiguration : IEntityTypeConfiguration<ProgramacionBateria>
{
    public void Configure(EntityTypeBuilder<ProgramacionBateria> builder)
    {
        builder.Property(p => p.Estado).HasDefaultValue("PendienteRevision");
        // El proveedor MySQL entrega date/time como DateTime/TimeSpan al materializar.
        builder.Property(p => p.Fecha).HasConversion(
            fecha => fecha.ToDateTime(TimeOnly.MinValue), fecha => DateOnly.FromDateTime(fecha)).HasColumnType("date");
        builder.Property(p => p.HoraInicio).HasConversion(
            hora => hora.ToTimeSpan(), hora => TimeOnly.FromTimeSpan(hora)).HasColumnType("time");
        builder.Property(p => p.HoraFin).HasConversion(
            hora => hora.ToTimeSpan(), hora => TimeOnly.FromTimeSpan(hora)).HasColumnType("time");
        builder.HasIndex(p => new { p.InicioUtc, p.FinUtc });
        builder.HasOne(p => p.Bateria).WithMany().HasForeignKey(p => p.IdBateria).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.CreadoPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.ModificadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProgramacionPozoConfiguration : IEntityTypeConfiguration<ProgramacionPozo>
{
    public void Configure(EntityTypeBuilder<ProgramacionPozo> builder)
    {
        builder.ToTable("programacion_pozo");
        builder.HasKey(p => new { p.IdProgramacion, p.IdPozo });
        builder.Property(p => p.IdProgramacion).HasColumnName("id_programacion");
        builder.Property(p => p.IdPozo).HasColumnName("id_pozo");
        builder.Property(p => p.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(p => p.CaudalM3H).HasColumnName("caudal_m3_h").HasPrecision(10, 2);
        builder.HasOne(p => p.Programacion).WithMany(p => p.Pozos).HasForeignKey(p => p.IdProgramacion).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Pozo).WithMany().HasForeignKey(p => p.IdPozo).OnDelete(DeleteBehavior.Restrict);
    }
}
