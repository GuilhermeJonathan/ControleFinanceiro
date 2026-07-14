using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class MoedaParamConfiguration : IEntityTypeConfiguration<MoedaParam>
{
    public void Configure(EntityTypeBuilder<MoedaParam> builder)
    {
        builder.ToTable("MoedasParam");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Codigo).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CotacaoBRL).HasPrecision(18, 6);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}
