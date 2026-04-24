using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class SaldoContaRepository(AppDbContext context) : ISaldoContaRepository
{
    public async Task<SaldoConta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.SaldosContas.FindAsync([id], cancellationToken);

    public async Task<SaldoConta?> GetByBancoAsync(string banco, CancellationToken cancellationToken = default)
        => await context.SaldosContas.FirstOrDefaultAsync(s => s.Banco == banco, cancellationToken);

    public async Task<IEnumerable<SaldoConta>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.SaldosContas.OrderBy(s => s.Banco).ToListAsync(cancellationToken);

    public async Task AddAsync(SaldoConta saldo, CancellationToken cancellationToken = default)
        => await context.SaldosContas.AddAsync(saldo, cancellationToken);

    public void Update(SaldoConta saldo) => context.SaldosContas.Update(saldo);
    public void Delete(SaldoConta saldo) => context.SaldosContas.Remove(saldo);
}
