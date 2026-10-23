using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class AcueductoConfiguration : IEntityTypeConfiguration<Acueducto>
{
    public void Configure(EntityTypeBuilder<Acueducto> builder)
    {
        builder.HasIndex(entidad => entidad.IdFuente)
            .IsUnique()
            .HasDatabaseName("IX_acueducto_id_fuente_unico");

        builder.HasOne(acueducto => acueducto.FuenteAgua)
            .WithOne()
            .HasForeignKey<Acueducto>(acueducto => acueducto.IdFuente);
    }
}
