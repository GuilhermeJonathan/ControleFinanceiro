using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class UserRestrictionConfiguration : IEntityTypeConfiguration<UserRestriction>
{
    public void Configure(EntityTypeBuilder<UserRestriction> builder)
    {
        builder.ToTable("UserRestrictions");
        builder.HasKey(r => new { r.UserId, r.ModuleId, r.CompanyId });
    }
}
