using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class SaldoContaConfiguration : IEntityTypeConfiguration<SaldoConta>
{
    public void Configure(EntityTypeBuilder<SaldoConta> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Banco).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Saldo).HasPrecision(18, 2);
        builder.Property(s => s.Tipo).IsRequired();

        builder.Property(s => s.UsuarioId).IsRequired();
        builder.HasIndex(s => s.UsuarioId);
    }
}
