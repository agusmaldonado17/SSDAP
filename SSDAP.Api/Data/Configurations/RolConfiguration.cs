using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.HasIndex(rol => rol.Codigo).IsUnique();
        builder.Property(rol => rol.Nombre).UseCollation("utf8mb4_unicode_ci");
        builder.HasIndex(rol => rol.Nombre).IsUnique();
    }
}
