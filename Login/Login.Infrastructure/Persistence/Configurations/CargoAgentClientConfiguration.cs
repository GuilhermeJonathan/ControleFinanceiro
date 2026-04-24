using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Login.Infrastructure.Persistence.Configurations;

public class CargoAgentClientConfiguration : IEntityTypeConfiguration<CargoAgentClient>
{
    public void Configure(EntityTypeBuilder<CargoAgentClient> builder)
    {
        builder.ToTable("CargoAgentClients");
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.Permissions)
            .WithOne()
            .HasForeignKey<CargoAgentPermission>(p => p.CargoAgentId);
    }
}
