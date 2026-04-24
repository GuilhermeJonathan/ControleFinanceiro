using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class CartaoCreditoConfiguration : IEntityTypeConfiguration<CartaoCredito>
{
    public void Configure(EntityTypeBuilder<CartaoCredito> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(100);
        builder.Property(c => c.DiaVencimento).IsRequired(false);
        builder.HasMany(c => c.Parcelas).WithOne(p => p.CartaoCredito)
            .HasForeignKey(p => p.CartaoCreditoId).OnDelete(DeleteBehavior.Cascade);
    }
}
