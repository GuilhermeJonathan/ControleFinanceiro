using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class InviteConfiguration : IEntityTypeConfiguration<Invite>
{
    public void Configure(EntityTypeBuilder<Invite> builder)
    {
        builder.ToTable("Invites");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Token).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Email).HasMaxLength(200);
        builder.Property(i => i.ExpiresAt).IsRequired();
        builder.Property(i => i.CreatedByUserId).IsRequired();
        builder.Property(i => i.UsedAt);

        builder.HasIndex(i => i.Token).IsUnique();
    }
}
