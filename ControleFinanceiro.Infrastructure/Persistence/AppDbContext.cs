using System.Reflection;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<CartaoCredito> CartoesCredito => Set<CartaoCredito>();
    public DbSet<ParcelaCartao> ParcelasCartao => Set<ParcelaCartao>();
    public DbSet<SaldoConta> SaldosContas => Set<SaldoConta>();
    public DbSet<HorasTrabalhadas> HorasTrabalhadas => Set<HorasTrabalhadas>();
    public DbSet<ReceitaRecorrente> ReceitasRecorrentes => Set<ReceitaRecorrente>();
    public DbSet<VinculoFamiliar> VinculosFamiliares => Set<VinculoFamiliar>();
    public DbSet<Meta> Metas => Set<Meta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public new async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}
