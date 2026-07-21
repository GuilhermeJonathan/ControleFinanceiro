using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ITipoInvestimentoParamRepository
{
    Task<List<TipoInvestimentoParam>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Somente os tipos globais (AssessorId == null) — catálogo do admin.</summary>
    Task<List<TipoInvestimentoParam>> GetGlobaisAsync(CancellationToken ct = default);
    /// <summary>Globais + os custom da assessoria informada.</summary>
    Task<List<TipoInvestimentoParam>> GetGlobaisEDoAssessorAsync(Guid assessorId, CancellationToken ct = default);
    Task<TipoInvestimentoParam?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(TipoInvestimentoParam entity, CancellationToken ct = default);
    void Remove(TipoInvestimentoParam entity);
}
