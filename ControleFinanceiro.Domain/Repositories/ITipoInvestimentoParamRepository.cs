using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ITipoInvestimentoParamRepository
{
    Task<List<TipoInvestimentoParam>> GetAllAsync(CancellationToken ct = default);
    Task<TipoInvestimentoParam?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(TipoInvestimentoParam entity, CancellationToken ct = default);
    void Remove(TipoInvestimentoParam entity);
}
