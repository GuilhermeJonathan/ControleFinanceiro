using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Document).IsRequired().HasMaxLength(14);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Occupation).HasMaxLength(200);
        builder.Property(u => u.Cellphone).HasMaxLength(20);
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.Region).HasMaxLength(100);

        builder.Property(u => u.PlanType)
               .HasConversion<int>()
               .HasDefaultValue(PlanType.None);
        builder.Property(u => u.TrialStartedAt);
        builder.Property(u => u.PlanExpiresAt);

        builder.Property(u => u.PodeVerImoveis).HasDefaultValue(false);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Document);

        builder.HasMany(u => u.Restrictions)
            .WithOne()
            .HasForeignKey(r => r.UserId);

        builder.HasOne(u => u.Profile)
            .WithMany()
            .HasForeignKey(u => u.ProfileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
