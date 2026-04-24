using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class HierarchyConfiguration : IEntityTypeConfiguration<Hierarchy>
{
    public void Configure(EntityTypeBuilder<Hierarchy> builder)
    {
        builder.ToTable("Hierarchies");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(h => h.Companies)
            .WithOne()
            .HasForeignKey(c => c.HierarchyId);
    }
}
