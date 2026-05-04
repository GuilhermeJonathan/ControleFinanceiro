using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class MercadoPagoSubscriptionConfiguration : IEntityTypeConfiguration<MercadoPagoSubscription>
{
    public void Configure(EntityTypeBuilder<MercadoPagoSubscription> builder)
    {
        builder.ToTable("MercadoPagoSubscriptions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.MpSubscriptionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(s => s.LastPaymentId)
            .HasMaxLength(100);

        builder.HasIndex(s => s.MpSubscriptionId).IsUnique();
        builder.HasIndex(s => s.UserId);
    }
}
