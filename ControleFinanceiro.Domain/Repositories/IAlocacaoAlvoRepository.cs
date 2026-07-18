using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IAlocacaoAlvoRepository
{
    Task<IEnumerable<AlocacaoAlvo>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<AlocacaoAlvo> alvos, CancellationToken ct = default);
    Task RemoveByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
}
