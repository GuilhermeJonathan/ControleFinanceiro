using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class PrecoAtivoHistoricoConfiguration : IEntityTypeConfiguration<PrecoAtivoHistorico>
{
    public void Configure(EntityTypeBuilder<PrecoAtivoHistorico> builder)
    {
        builder.ToTable("PrecosAtivoHistorico");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Ticker).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Preco).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Fonte).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DataHora).IsRequired();
        builder.HasIndex(x => new { x.Ticker, x.DataHora });
    }
}
