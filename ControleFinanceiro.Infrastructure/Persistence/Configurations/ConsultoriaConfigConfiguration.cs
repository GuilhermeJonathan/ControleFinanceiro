using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ConsultoriaConfigConfiguration : IEntityTypeConfiguration<ConsultoriaConfig>
{
    public void Configure(EntityTypeBuilder<ConsultoriaConfig> builder)
    {
        builder.ToTable("ConsultoriaConfigs");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.UsuarioId).IsUnique();
        builder.Property(c => c.NomeConsultoria).IsRequired().HasMaxLength(200);
        builder.Property(c => c.LogoBase64);   // data URL — texto sem limite
        builder.Property(c => c.CorMarca).HasMaxLength(20);
        builder.Property(c => c.WhatsApp).HasMaxLength(30);
        builder.Property(c => c.MensagemRodape).HasMaxLength(500);
    }
}
