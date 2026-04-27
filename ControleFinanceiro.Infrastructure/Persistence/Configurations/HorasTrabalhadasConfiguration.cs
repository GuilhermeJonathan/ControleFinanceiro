using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class HorasTrabalhadasConfiguration : IEntityTypeConfiguration<HorasTrabalhadas>
{
    public void Configure(EntityTypeBuilder<HorasTrabalhadas> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(h => h.ValorHora).HasPrecision(18, 2);
        builder.Property(h => h.Quantidade).HasPrecision(10, 2);
        builder.Ignore(h => h.ValorTotal);

        builder.Property(h => h.UsuarioId).IsRequired();
        builder.HasIndex(h => h.UsuarioId);
    }
}
