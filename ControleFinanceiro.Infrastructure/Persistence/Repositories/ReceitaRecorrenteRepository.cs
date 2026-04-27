using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ReceitaRecorrenteRepository(AppDbContext context) : IReceitaRecorrenteRepository
{
    public async Task<IEnumerable<ReceitaRecorrente>> GetAllAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.ReceitasRecorrentes
            .Where(r => r.UsuarioId == usuarioId)
            .OrderBy(r => r.Nome)
            .ToListAsync(cancellationToken);

    public async Task<ReceitaRecorrente?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.ReceitasRecorrentes.FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId, cancellationToken);

    public async Task AddAsync(ReceitaRecorrente receitaRecorrente, CancellationToken cancellationToken = default)
        => await context.ReceitasRecorrentes.AddAsync(receitaRecorrente, cancellationToken);

    public void Update(ReceitaRecorrente receitaRecorrente)
        => context.ReceitasRecorrentes.Update(receitaRecorrente);

    public void Delete(ReceitaRecorrente receitaRecorrente)
        => context.ReceitasRecorrentes.Remove(receitaRecorrente);
}
