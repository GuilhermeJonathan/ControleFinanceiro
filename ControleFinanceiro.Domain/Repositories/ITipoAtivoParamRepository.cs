using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ITipoAtivoParamRepository
{
    Task<List<TipoAtivoParam>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Somente os tipos globais (AssessorId == null) — catálogo do admin.</summary>
    Task<List<TipoAtivoParam>> GetGlobaisAsync(CancellationToken ct = default);
    /// <summary>Globais + os custom da assessoria informada.</summary>
    Task<List<TipoAtivoParam>> GetGlobaisEDoAssessorAsync(Guid assessorId, CancellationToken ct = default);
    Task<TipoAtivoParam?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(TipoAtivoParam entity, CancellationToken ct = default);
    void Remove(TipoAtivoParam entity);
}
