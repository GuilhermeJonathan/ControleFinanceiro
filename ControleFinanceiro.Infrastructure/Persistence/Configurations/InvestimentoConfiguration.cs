using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class InvestimentoConfiguration : IEntityTypeConfiguration<Investimento>
{
    public void Configure(EntityTypeBuilder<Investimento> builder)
    {
        builder.ToTable("Investimentos");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Nome).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Tipo).HasConversion<int>();
        builder.Property(i => i.Moeda).HasConversion<int>();
        builder.Property(i => i.Subclasse).HasMaxLength(60);
        builder.Property(i => i.Corretora).HasMaxLength(100);
        builder.Property(i => i.Ticker).HasMaxLength(20);
        builder.Property(i => i.Quantidade).HasPrecision(18, 6);
        builder.Property(i => i.ValorAplicado).HasPrecision(18, 2);
        builder.Property(i => i.ValorAtual).HasPrecision(18, 2);
        builder.Property(i => i.RentabilidadeAnualPct).HasPrecision(9, 4);
        builder.HasIndex(i => i.UsuarioId);
    }
}
