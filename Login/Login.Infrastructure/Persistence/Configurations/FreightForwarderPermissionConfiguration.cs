using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class FreightForwarderPermissionConfiguration : IEntityTypeConfiguration<FreightForwarderPermission>
{
    public void Configure(EntityTypeBuilder<FreightForwarderPermission> builder)
    {
        builder.ToTable("FreightForwarderPermissions");
        builder.HasKey(p => p.FreightForwarderId);
    }
}
