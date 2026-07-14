using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class AtivoPatrimonialConfiguration : IEntityTypeConfiguration<AtivoPatrimonial>
{
    public void Configure(EntityTypeBuilder<AtivoPatrimonial> builder)
    {
        builder.ToTable("AtivosPatrimoniais");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Nome).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Tipo).HasConversion<int>();
        builder.Property(a => a.Moeda).HasConversion<int>();
        builder.Property(a => a.ValorAtual).HasPrecision(18, 2);
        builder.Property(a => a.ValorizacaoAnualPct).HasPrecision(9, 4);
        builder.Property(a => a.ReceitaMensal).HasPrecision(18, 2);
        builder.Property(a => a.DespesaMensal).HasPrecision(18, 2);
        builder.HasIndex(a => a.UsuarioId);
    }
}
