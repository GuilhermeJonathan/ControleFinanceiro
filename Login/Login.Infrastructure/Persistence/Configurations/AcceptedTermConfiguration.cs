using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class AcceptedTermConfiguration : IEntityTypeConfiguration<AcceptedTerm>
{
    public void Configure(EntityTypeBuilder<AcceptedTerm> builder)
    {
        builder.ToTable("AcceptedTerms");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TermName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.IpAddress).HasMaxLength(50);
        builder.Property(t => t.UserAgent).HasMaxLength(500);

        builder.HasIndex(t => new { t.UserId, t.TermName });
    }
}
