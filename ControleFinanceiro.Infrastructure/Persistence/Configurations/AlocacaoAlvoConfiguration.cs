using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class AlocacaoAlvoConfiguration : IEntityTypeConfiguration<AlocacaoAlvo>
{
    public void Configure(EntityTypeBuilder<AlocacaoAlvo> b)
    {
        b.ToTable("AlocacoesAlvo");
        b.HasKey(a => a.Id);
        b.Property(a => a.Tipo).HasConversion<int>();
        b.Property(a => a.PercentualAlvo).HasColumnType("numeric(5,2)");
        b.HasIndex(a => new { a.UsuarioId, a.Tipo }).IsUnique();
    }
}
