using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ParametroOcultoConfiguration : IEntityTypeConfiguration<ParametroOculto>
{
    public void Configure(EntityTypeBuilder<ParametroOculto> builder)
    {
        builder.ToTable("ParametrosOcultos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Tipo).HasConversion<int>();
        builder.HasIndex(x => new { x.AssessorId, x.Tipo, x.ParametroId }).IsUnique();
    }
}
