using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class ParametroLeyConfiguration : IEntityTypeConfiguration<ParametroLey>
{
    public void Configure(EntityTypeBuilder<ParametroLey> builder)
    {
        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.Property(p => p.Codigo).UseCollation("utf8mb4_bin");
    }
}
