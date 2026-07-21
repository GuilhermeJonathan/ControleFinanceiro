using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IContaFinanceiraRepository
{
    Task<List<ContaFinanceira>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<ContaFinanceira?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ContaFinanceira entity, CancellationToken ct = default);
    void Remove(ContaFinanceira entity);
}
