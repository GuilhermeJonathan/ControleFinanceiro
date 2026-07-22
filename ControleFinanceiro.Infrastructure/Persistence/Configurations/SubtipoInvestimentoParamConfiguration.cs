using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class SubtipoInvestimentoParamConfiguration : IEntityTypeConfiguration<SubtipoInvestimentoParam>
{
    public void Configure(EntityTypeBuilder<SubtipoInvestimentoParam> builder)
    {
        builder.ToTable("SubtiposInvestimentoParam");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(80);
        builder.HasIndex(x => x.TipoInvestimentoId);
    }
}
