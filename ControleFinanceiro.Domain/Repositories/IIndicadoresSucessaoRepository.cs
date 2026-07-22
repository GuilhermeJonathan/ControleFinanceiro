using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IIndicadoresSucessaoRepository
{
    Task<IndicadoresSucessao?> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AddAsync(IndicadoresSucessao entity, CancellationToken ct = default);
}
