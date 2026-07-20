using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class CotacaoHistoricoConfiguration : IEntityTypeConfiguration<CotacaoHistorico>
{
    public void Configure(EntityTypeBuilder<CotacaoHistorico> builder)
    {
        builder.ToTable("CotacoesHistorico");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.MoedaCodigo).HasMaxLength(10).IsRequired();
        builder.Property(c => c.CotacaoBRL).HasPrecision(18, 6).IsRequired();
        builder.Property(c => c.Fonte).HasMaxLength(50).IsRequired();
        builder.Property(c => c.DataHora).IsRequired();

        builder.HasIndex(c => new { c.MoedaCodigo, c.DataHora });
    }
}
