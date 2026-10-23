using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class PozoConfiguration : IEntityTypeConfiguration<Pozo>
{
    public void Configure(EntityTypeBuilder<Pozo> builder)
    {
        builder.HasIndex(entidad => entidad.IdFuente)
            .IsUnique()
            .HasDatabaseName("IX_pozo_id_fuente_unico");

        builder.HasOne(pozo => pozo.FuenteAgua)
            .WithOne()
            .HasForeignKey<Pozo>(pozo => pozo.IdFuente);
    }
}
