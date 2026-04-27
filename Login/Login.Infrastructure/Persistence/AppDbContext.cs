using System.Reflection;
using Login.Domain.Common;
using Login.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using DomainModule = Login.Domain.Entities.Module;

namespace Login.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Invite> Invites => Set<Invite>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<DomainModule> Modules => Set<DomainModule>();
    public DbSet<ModuleFunction> ModuleFunctions => Set<ModuleFunction>();
    public DbSet<AcceptedTerm> AcceptedTerms => Set<AcceptedTerm>();
    public DbSet<UserRestriction> UserRestrictions => Set<UserRestriction>();
    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}
