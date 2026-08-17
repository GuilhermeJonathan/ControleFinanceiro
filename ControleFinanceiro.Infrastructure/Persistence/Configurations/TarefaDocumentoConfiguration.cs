using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class TarefaDocumentoConfiguration : IEntityTypeConfiguration<TarefaDocumento>
{
    public void Configure(EntityTypeBuilder<TarefaDocumento> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Titulo).HasMaxLength(300);
        b.Property(x => x.Descricao).HasMaxLength(2000);
        b.Property(x => x.AtalhoRota).HasMaxLength(60);
        b.HasIndex(x => x.ClienteId);
        b.HasIndex(x => new { x.AssessorId, x.ClienteId });
    }
}
