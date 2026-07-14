using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class PassivoPatrimonialConfiguration : IEntityTypeConfiguration<PassivoPatrimonial>
{
    public void Configure(EntityTypeBuilder<PassivoPatrimonial> builder)
    {
        builder.ToTable("PassivosPatrimoniais");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Moeda).HasConversion<int>();
        builder.Property(p => p.Prazo).HasConversion<int>();
        builder.Property(p => p.Valor).HasPrecision(18, 2);
        builder.Property(p => p.TaxaJurosAnualPct).HasPrecision(9, 4);
        builder.HasIndex(p => p.UsuarioId);
    }
}
