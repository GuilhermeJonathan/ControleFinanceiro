using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ISubtipoInvestimentoParamRepository
{
    Task<List<SubtipoInvestimentoParam>> GetAllAsync(CancellationToken ct = default);
    Task<List<SubtipoInvestimentoParam>> GetByTipoAsync(int tipoInvestimentoId, CancellationToken ct = default);
    Task<SubtipoInvestimentoParam?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(SubtipoInvestimentoParam entity, CancellationToken ct = default);
    void Remove(SubtipoInvestimentoParam entity);
}
