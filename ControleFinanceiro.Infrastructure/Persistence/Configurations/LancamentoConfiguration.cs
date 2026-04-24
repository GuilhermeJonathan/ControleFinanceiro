using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class LancamentoConfiguration : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Valor).HasPrecision(18, 2);
        builder.Property(l => l.Tipo).IsRequired();
        builder.Property(l => l.Situacao).IsRequired();

        builder.HasOne(l => l.Categoria).WithMany(c => c.Lancamentos)
            .HasForeignKey(l => l.CategoriaId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.Cartao).WithMany()
            .HasForeignKey(l => l.CartaoId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.ReceitaRecorrente).WithMany()
            .HasForeignKey(l => l.ReceitaRecorrenteId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.ContaBancaria).WithMany()
            .HasForeignKey(l => l.ContaBancariaId).OnDelete(DeleteBehavior.SetNull);
    }
}
