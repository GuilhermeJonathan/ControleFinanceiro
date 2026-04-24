using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class CargoAgentPermissionConfiguration : IEntityTypeConfiguration<CargoAgentPermission>
{
    public void Configure(EntityTypeBuilder<CargoAgentPermission> builder)
    {
        builder.ToTable("CargoAgentPermissions");
        builder.HasKey(p => p.CargoAgentId);
    }
}
