using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class BateriaPozoConfiguration : IEntityTypeConfiguration<BateriaPozo>
{
    public void Configure(EntityTypeBuilder<BateriaPozo> builder)
    {
        builder.HasIndex(asociacion => new { asociacion.IdBateria, asociacion.IdPozo })
            .IsUnique();
    }
}
