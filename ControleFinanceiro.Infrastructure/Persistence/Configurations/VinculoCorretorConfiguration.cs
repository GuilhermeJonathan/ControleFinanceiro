using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class VinculoCorretorConfiguration : IEntityTypeConfiguration<VinculoCorretor>
{
    public void Configure(EntityTypeBuilder<VinculoCorretor> b)
    {
        b.ToTable("VinculosCorretor");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoConvite).HasMaxLength(10).IsRequired();
        b.Property(x => x.NomeAssessor).HasMaxLength(200);
        b.Property(x => x.NomeCorretor).HasMaxLength(200);
        b.Property(x => x.EmailConvidado).HasMaxLength(200);
        b.HasIndex(x => x.CodigoConvite).IsUnique();
        b.HasIndex(x => x.AssessorId);
        b.HasIndex(x => x.CorretorId);
    }
}
