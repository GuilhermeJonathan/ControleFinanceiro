using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ImovelFotoConfiguration : IEntityTypeConfiguration<ImovelFoto>
{
    public void Configure(EntityTypeBuilder<ImovelFoto> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Dados).IsRequired().HasColumnType("text");
    }
}
