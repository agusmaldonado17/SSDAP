using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class MedicionCloroConfiguration : IEntityTypeConfiguration<MedicionCloro>
{
    public void Configure(EntityTypeBuilder<MedicionCloro> builder)
    {
        builder.ToTable("medicion_cloro");
        builder.HasKey(m => m.IdMedicionCloro);
        builder.Property(m => m.IdMedicionCloro).HasColumnName("id_medicion_cloro");
        builder.Property(m => m.IdLocalidad).HasColumnName("id_localidad");
        builder.Property(m => m.FechaHoraUtc).HasColumnName("fecha_hora_utc").HasColumnType("datetime(6)");
        builder.Property(m => m.ValorMgL).HasColumnName("valor_mg_l").HasPrecision(14, 6);
        builder.Property(m => m.TipoCloro).HasColumnName("tipo_cloro").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Observaciones).HasColumnName("observaciones").HasMaxLength(255);
        builder.Property(m => m.CreadoEnUtc).HasColumnName("creado_en_utc").HasColumnType("datetime(6)");
        builder.Property(m => m.CreadoPorId).HasColumnName("creado_por_id");
        builder.HasIndex(m => new { m.IdLocalidad, m.FechaHoraUtc, m.IdMedicionCloro });
        builder.HasOne<Localidad>().WithMany().HasForeignKey(m => m.IdLocalidad).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(m => m.CreadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}
