using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IPlanoAcaoRepository
{
    /// <summary>Todos os planos do usuário (um cliente pode ter vários).</summary>
    Task<IEnumerable<PlanoAcao>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<PlanoAcao?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PlanoAcao plano, CancellationToken ct = default);
    void Update(PlanoAcao plano);
    void Remove(PlanoAcao plano);
}
