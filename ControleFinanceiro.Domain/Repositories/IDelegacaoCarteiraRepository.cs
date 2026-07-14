using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IDelegacaoCarteiraRepository
{
    Task<IEnumerable<DelegacaoCarteira>> GetByAssessorAsync(Guid assessorId, CancellationToken ct);
    Task<IEnumerable<DelegacaoCarteira>> GetByCorretorAsync(Guid corretorId, CancellationToken ct);
    Task<DelegacaoCarteira?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<DelegacaoCarteira?> GetAtivaAsync(Guid corretorId, Guid clienteId, CancellationToken ct);
    Task<bool> ExisteAtivaAsync(Guid corretorId, Guid clienteId, CancellationToken ct);
    Task AddAsync(DelegacaoCarteira delegacao, CancellationToken ct);
    void Update(DelegacaoCarteira delegacao);
}
