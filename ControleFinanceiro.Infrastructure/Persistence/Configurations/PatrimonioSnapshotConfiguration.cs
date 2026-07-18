using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class PatrimonioSnapshotConfiguration : IEntityTypeConfiguration<PatrimonioSnapshot>
{
    public void Configure(EntityTypeBuilder<PatrimonioSnapshot> b)
    {
        b.ToTable("PatrimonioSnapshots");
        b.HasKey(s => s.Id);
        b.Property(s => s.PatrimonioLiquidoBRL).HasColumnType("numeric(18,2)");
        b.Property(s => s.TotalBensBRL).HasColumnType("numeric(18,2)");
        b.Property(s => s.TotalDividasBRL).HasColumnType("numeric(18,2)");
        b.HasIndex(s => new { s.UsuarioId, s.Ano, s.Mes }).IsUnique();
    }
}
