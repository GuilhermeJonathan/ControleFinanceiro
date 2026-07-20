using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ParametrosSaudeConfiguration : IEntityTypeConfiguration<ParametrosSaude>
{
    public void Configure(EntityTypeBuilder<ParametrosSaude> builder)
    {
        builder.ToTable("ParametrosSaude");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AssessorId).IsUnique();
    }
}
