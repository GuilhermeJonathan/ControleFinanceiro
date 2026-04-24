using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IReceitaRecorrenteRepository
{
    Task<IEnumerable<ReceitaRecorrente>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ReceitaRecorrente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ReceitaRecorrente receitaRecorrente, CancellationToken cancellationToken = default);
    void Update(ReceitaRecorrente receitaRecorrente);
    void Delete(ReceitaRecorrente receitaRecorrente);
}
