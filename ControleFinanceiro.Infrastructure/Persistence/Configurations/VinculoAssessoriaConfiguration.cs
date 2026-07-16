using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class VinculoAssessoriaConfiguration : IEntityTypeConfiguration<VinculoAssessoria>
{
    public void Configure(EntityTypeBuilder<VinculoAssessoria> builder)
    {
        builder.ToTable("VinculosAssessoria");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.CodigoConvite).HasMaxLength(10).IsRequired();
        builder.HasIndex(v => v.CodigoConvite).IsUnique();
        builder.HasIndex(v => v.AssessorId);
        builder.HasIndex(v => v.ClienteId);
        builder.Property(v => v.NomeCliente).HasMaxLength(200);
        builder.Property(v => v.NomeAssessor).HasMaxLength(200);
        builder.Property(v => v.EmailConvidado).HasMaxLength(200);
        builder.Ignore(v => v.Ativo);
    }
}
