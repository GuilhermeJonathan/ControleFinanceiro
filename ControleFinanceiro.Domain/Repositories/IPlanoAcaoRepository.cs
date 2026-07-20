using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IPlanoAcaoRepository
{
    Task<PlanoAcao?> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AddAsync(PlanoAcao plano, CancellationToken ct = default);
    void Update(PlanoAcao plano);
}
