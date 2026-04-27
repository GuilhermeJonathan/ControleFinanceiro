using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ReceitaRecorrenteConfiguration : IEntityTypeConfiguration<ReceitaRecorrente>
{
    public void Configure(EntityTypeBuilder<ReceitaRecorrente> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nome).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Tipo).IsRequired();
        builder.Property(r => r.Valor).HasPrecision(18, 2);
        builder.Property(r => r.ValorHora).HasPrecision(18, 2).IsRequired(false);
        builder.Property(r => r.QuantidadeHoras).HasPrecision(18, 2).IsRequired(false);
        builder.Property(r => r.Dia).IsRequired();
        builder.Property(r => r.DataInicio).IsRequired();

        builder.Property(r => r.UsuarioId).IsRequired();
        builder.HasIndex(r => r.UsuarioId);
    }
}
