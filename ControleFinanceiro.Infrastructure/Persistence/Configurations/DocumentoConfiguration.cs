using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Alvo).HasConversion<int>();
        b.Property(x => x.Nome).HasMaxLength(400);
        b.Property(x => x.StoragePath).HasMaxLength(1024);
        b.Property(x => x.ContentType).HasMaxLength(200);
        b.Property(x => x.Categoria).HasMaxLength(200);
        b.HasIndex(x => x.UsuarioId);
        b.HasIndex(x => new { x.UsuarioId, x.Alvo, x.AlvoId });
    }
}
