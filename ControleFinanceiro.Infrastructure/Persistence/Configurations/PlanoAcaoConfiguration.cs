using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class PlanoAcaoConfiguration : IEntityTypeConfiguration<PlanoAcao>
{
    public void Configure(EntityTypeBuilder<PlanoAcao> builder)
    {
        builder.ToTable("PlanosAcao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Objetivo).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Prazo).HasMaxLength(60);
        builder.HasIndex(x => x.UsuarioId);

        builder.OwnsMany(x => x.Etapas, eb =>
        {
            eb.ToTable("EtapasPlano");
            eb.WithOwner().HasForeignKey("PlanoAcaoId");
            eb.Property<int>("Id");
            eb.HasKey("Id");
            eb.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            eb.Property(e => e.Descricao).HasMaxLength(1000);
            eb.Property(e => e.Prazo).HasMaxLength(60);
            eb.Property(e => e.Alvo).HasMaxLength(120);
            eb.Property(e => e.Status).HasConversion<int>();
        });

        builder.Metadata.FindNavigation(nameof(PlanoAcao.Etapas))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
