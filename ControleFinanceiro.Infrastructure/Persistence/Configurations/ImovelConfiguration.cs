using System.Text.Json;
using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class ImovelConfiguration : IEntityTypeConfiguration<Imovel>
{
    public void Configure(EntityTypeBuilder<Imovel> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Descricao).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Valor).HasPrecision(18, 2);
        builder.Property(i => i.NomeCorretor).HasMaxLength(200);
        builder.Property(i => i.TelefoneCorretor).HasMaxLength(50);
        builder.Property(i => i.Imobiliaria).HasMaxLength(200);
        builder.Property(i => i.Tipo).HasMaxLength(50);

        builder.Property(i => i.Pros)
            .HasColumnType("text")
            .HasConversion(
                v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions)) ?? new());

        builder.Property(i => i.Contras)
            .HasColumnType("text")
            .HasConversion(
                v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions)) ?? new());

        builder.HasMany(i => i.Fotos)
            .WithOne(f => f.Imovel)
            .HasForeignKey(f => f.ImovelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Comentarios)
            .WithOne(c => c.Imovel)
            .HasForeignKey(c => c.ImovelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
