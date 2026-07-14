using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class DelegacaoCarteiraConfiguration : IEntityTypeConfiguration<DelegacaoCarteira>
{
    public void Configure(EntityTypeBuilder<DelegacaoCarteira> b)
    {
        b.ToTable("DelegacoesCarteira");
        b.HasKey(x => x.Id);
        b.Property(x => x.NomeCliente).HasMaxLength(200);
        b.Property(x => x.NomeCorretor).HasMaxLength(200);
        b.HasIndex(x => x.AssessorId);
        b.HasIndex(x => x.CorretorId);
        b.HasIndex(x => new { x.CorretorId, x.ClienteId });
    }
}
