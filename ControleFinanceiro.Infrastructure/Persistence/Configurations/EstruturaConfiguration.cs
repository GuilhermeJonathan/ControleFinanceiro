using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class EstruturaConfiguration : IEntityTypeConfiguration<Estrutura>
{
    public void Configure(EntityTypeBuilder<Estrutura> builder)
    {
        builder.ToTable("Estruturas");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Tipo).HasConversion<int>();
        builder.Property(e => e.Jurisdicao).HasMaxLength(120);
        builder.Property(e => e.Observacoes).HasMaxLength(2000);
        builder.HasIndex(e => e.UsuarioId);
    }
}

public class ParticipacaoEstruturaConfiguration : IEntityTypeConfiguration<ParticipacaoEstrutura>
{
    public void Configure(EntityTypeBuilder<ParticipacaoEstrutura> builder)
    {
        builder.ToTable("ParticipacoesEstrutura");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PercentualParticipacao).HasPrecision(9, 4);
        builder.Property(p => p.TipoRelacao).HasConversion<int>();
        builder.HasIndex(p => p.UsuarioId);
        // Uma aresta única por par (pai, filha) dentro do usuário.
        builder.HasIndex(p => new { p.UsuarioId, p.EstruturaPaiId, p.EstruturaFilhaId }).IsUnique();
    }
}

public class BeneficiarioConfiguration : IEntityTypeConfiguration<Beneficiario>
{
    public void Configure(EntityTypeBuilder<Beneficiario> builder)
    {
        builder.ToTable("Beneficiarios");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Nome).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Papel).HasConversion<int>();
        builder.Property(b => b.PercentualDistribuicao).HasPrecision(9, 4);
        builder.Property(b => b.CondicaoLiberacao).HasMaxLength(500);
        builder.HasIndex(b => b.UsuarioId);
    }
}

public class DistribuicaoConfiguration : IEntityTypeConfiguration<Distribuicao>
{
    public void Configure(EntityTypeBuilder<Distribuicao> builder)
    {
        builder.ToTable("Distribuicoes");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Valor).HasPrecision(18, 2);
        builder.Property(d => d.Moeda).HasConversion<int>();
        builder.Property(d => d.Descricao).HasMaxLength(500);
        builder.HasIndex(d => d.UsuarioId);
    }
}
