using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class VinculoFamiliarConfiguration : IEntityTypeConfiguration<VinculoFamiliar>
{
    public void Configure(EntityTypeBuilder<VinculoFamiliar> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.CodigoConvite).HasMaxLength(10).IsRequired();
        builder.HasIndex(v => v.CodigoConvite).IsUnique();
        builder.HasIndex(v => v.MembroId);
        builder.Property(v => v.NomeMembro).HasMaxLength(200);
    }
}
