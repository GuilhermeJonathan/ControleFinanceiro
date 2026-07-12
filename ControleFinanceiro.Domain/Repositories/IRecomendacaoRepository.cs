using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IRecomendacaoRepository
{
    Task<Recomendacao?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Recomendacao>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<IEnumerable<Recomendacao>> GetByAssessorEClienteAsync(Guid assessorId, Guid clienteId, CancellationToken ct = default);
    Task<int> CountPendentesByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task AddAsync(Recomendacao recomendacao, CancellationToken ct = default);
    void Remove(Recomendacao recomendacao);
}
