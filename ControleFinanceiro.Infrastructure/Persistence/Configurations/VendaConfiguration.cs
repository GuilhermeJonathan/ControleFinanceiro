using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Descricao).HasMaxLength(300).IsRequired();
        builder.Property(v => v.Valor).HasPrecision(18, 2);
        builder.Property(v => v.Status).HasConversion<int>();
        builder.Property(v => v.Origem).HasConversion<int>();
        builder.Property(v => v.CriadoPorNome).HasMaxLength(200).HasDefaultValue("");
        builder.HasIndex(v => v.UsuarioId);
        builder.HasIndex(v => v.Data);
    }
}
