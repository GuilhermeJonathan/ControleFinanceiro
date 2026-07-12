using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class RecomendacaoConfiguration : IEntityTypeConfiguration<Recomendacao>
{
    public void Configure(EntityTypeBuilder<Recomendacao> builder)
    {
        builder.ToTable("Recomendacoes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Texto).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.RespostaCliente).HasMaxLength(1000);
        builder.Property(r => r.Tipo).HasConversion<int>();
        builder.Property(r => r.Status).HasConversion<int>();
        builder.HasIndex(r => r.ClienteId);
        builder.HasIndex(r => new { r.AssessorId, r.ClienteId });
    }
}
