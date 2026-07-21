using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ContaFinanceiraConfiguration : IEntityTypeConfiguration<ContaFinanceira>
{
    public void Configure(EntityTypeBuilder<ContaFinanceira> builder)
    {
        builder.ToTable("ContasFinanceiras");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Tipo).HasConversion<int>();
        builder.Property(c => c.Instituicao).HasMaxLength(120);
        builder.Property(c => c.Pais).HasMaxLength(80);
        builder.Property(c => c.Moeda).HasConversion<int>();
        builder.Property(c => c.Saldo).HasPrecision(18, 2);
        builder.Property(c => c.Identificador).HasMaxLength(120);
        builder.HasIndex(c => c.UsuarioId);
    }
}
