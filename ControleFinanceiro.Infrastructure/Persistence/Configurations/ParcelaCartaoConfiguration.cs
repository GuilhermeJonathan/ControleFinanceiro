using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ParcelaCartaoConfiguration : IEntityTypeConfiguration<ParcelaCartao>
{
    public void Configure(EntityTypeBuilder<ParcelaCartao> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ValorParcela).HasPrecision(18, 2);
    }
}
