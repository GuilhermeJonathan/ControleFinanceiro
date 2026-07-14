using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class TipoAtivoParamConfiguration : IEntityTypeConfiguration<TipoAtivoParam>
{
    public void Configure(EntityTypeBuilder<TipoAtivoParam> builder)
    {
        builder.ToTable("TiposAtivoParam");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Icone).HasMaxLength(10);
    }
}
