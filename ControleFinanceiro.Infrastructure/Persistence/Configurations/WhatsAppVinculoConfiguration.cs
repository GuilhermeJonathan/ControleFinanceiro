using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class WhatsAppVinculoConfiguration : IEntityTypeConfiguration<WhatsAppVinculo>
{
    public void Configure(EntityTypeBuilder<WhatsAppVinculo> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(v => v.UserId).IsRequired();

        // Um número só pode estar vinculado a um usuário e vice-versa
        builder.HasIndex(v => v.PhoneNumber).IsUnique();
        builder.HasIndex(v => v.UserId).IsUnique();
    }
}
