using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class IndicadoresSucessaoConfiguration : IEntityTypeConfiguration<IndicadoresSucessao>
{
    public void Configure(EntityTypeBuilder<IndicadoresSucessao> builder)
    {
        builder.ToTable("IndicadoresSucessao");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.UsuarioId).IsUnique();
    }
}
