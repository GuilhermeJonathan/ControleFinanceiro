using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class SimulacaoPatrimonialConfiguration : IEntityTypeConfiguration<SimulacaoPatrimonial>
{
    public void Configure(EntityTypeBuilder<SimulacaoPatrimonial> builder)
    {
        builder.ToTable("SimulacoesPatrimoniais");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PatrimonioInicial).HasPrecision(18, 2);
        builder.Property(x => x.AporteMensal).HasPrecision(18, 2);
        builder.Property(x => x.TaxaRetornoRealAnualPct).HasPrecision(9, 4);
        builder.Property(x => x.RetiradaMensal).HasPrecision(18, 2);
        builder.HasIndex(x => x.UsuarioId);

        builder.OwnsMany(x => x.Cenarios, cb =>
        {
            cb.ToTable("CenariosSimulacao");
            cb.WithOwner().HasForeignKey("SimulacaoId");
            cb.Property<int>("Id");
            cb.HasKey("Id");
            cb.Property(c => c.Nome).IsRequired().HasMaxLength(200);
            cb.Property(c => c.Tipo).HasConversion<int>();
            cb.Property(c => c.Valor).HasPrecision(18, 2);
        });

        builder.Metadata.FindNavigation(nameof(SimulacaoPatrimonial.Cenarios))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
