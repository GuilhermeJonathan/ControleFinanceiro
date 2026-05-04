using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.UserEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Amount)
            .HasColumnType("numeric(10,2)");

        builder.Property(t => t.MpPaymentId)
            .HasMaxLength(100);

        builder.Property(t => t.MpSubscriptionId)
            .HasMaxLength(100);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.MpPaymentId);
    }
}
