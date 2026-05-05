using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class MetaConfiguration : IEntityTypeConfiguration<Meta>
{
    public void Configure(EntityTypeBuilder<Meta> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Descricao).HasMaxLength(500);
        builder.Property(m => m.ValorMeta).HasPrecision(18, 2);
        builder.Property(m => m.ValorAtual).HasPrecision(18, 2);
        builder.Property(m => m.Capa).HasMaxLength(50);
        builder.Property(m => m.CorFundo).HasMaxLength(20);
        builder.Property(m => m.ContribuicaoMensalValor).HasPrecision(18, 2);
        builder.HasIndex(m => m.UsuarioId);
    }
}
