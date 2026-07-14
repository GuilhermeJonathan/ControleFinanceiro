using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IInvestimentoRepository
{
    Task<IEnumerable<Investimento>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Investimento?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Investimento investimento, CancellationToken ct = default);
    void Update(Investimento investimento);
    void Remove(Investimento investimento);
}
