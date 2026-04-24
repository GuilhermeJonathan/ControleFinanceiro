using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class FreightForwarderConfiguration : IEntityTypeConfiguration<FreightForwarder>
{
    public void Configure(EntityTypeBuilder<FreightForwarder> builder)
    {
        builder.ToTable("FreightForwarders");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Document).IsRequired().HasMaxLength(14);
        builder.Property(f => f.Email).HasMaxLength(256);

        builder.HasIndex(f => f.Document).IsUnique();

        builder.HasOne(f => f.Permissions)
            .WithOne()
            .HasForeignKey<FreightForwarderPermission>(p => p.FreightForwarderId);
    }
}
