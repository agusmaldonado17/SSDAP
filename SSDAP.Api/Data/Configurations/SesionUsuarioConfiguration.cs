using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class SesionUsuarioConfiguration : IEntityTypeConfiguration<SesionUsuario>
{
    public void Configure(EntityTypeBuilder<SesionUsuario> b)
    {
        b.ToTable("sesion_usuario");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).HasColumnName("id_sesion").HasMaxLength(36);
        b.Property(s => s.IdUsuario).HasColumnName("id_usuario");
        b.Property(s => s.CreadaEnUtc).HasColumnName("creada_en_utc").HasColumnType("datetime(6)");
        b.Property(s => s.UltimaActividadUtc).HasColumnName("ultima_actividad_utc").HasColumnType("datetime(6)");
        b.Property(s => s.ExpiraEnUtc).HasColumnName("expira_en_utc").HasColumnType("datetime(6)");
        b.HasIndex(s => s.ExpiraEnUtc);
        b.HasOne<Usuario>().WithMany().HasForeignKey(s => s.IdUsuario).OnDelete(DeleteBehavior.Cascade);
    }
}
