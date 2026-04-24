using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class HierarchyCompanyConfiguration : IEntityTypeConfiguration<HierarchyCompany>
{
    public void Configure(EntityTypeBuilder<HierarchyCompany> builder)
    {
        builder.ToTable("HierarchyCompanies");
        builder.HasKey(h => new { h.HierarchyId, h.ClientId });
    }
}
