using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ReceitaRecorrenteRepository(AppDbContext context) : IReceitaRecorrenteRepository
{
    public async Task<IEnumerable<ReceitaRecorrente>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.ReceitasRecorrentes
            .OrderBy(r => r.Nome)
            .ToListAsync(cancellationToken);

    public async Task<ReceitaRecorrente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ReceitasRecorrentes.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(ReceitaRecorrente receitaRecorrente, CancellationToken cancellationToken = default)
        => await context.ReceitasRecorrentes.AddAsync(receitaRecorrente, cancellationToken);

    public void Update(ReceitaRecorrente receitaRecorrente)
        => context.ReceitasRecorrentes.Update(receitaRecorrente);

    public void Delete(ReceitaRecorrente receitaRecorrente)
        => context.ReceitasRecorrentes.Remove(receitaRecorrente);
}
