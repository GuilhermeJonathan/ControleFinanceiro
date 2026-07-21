using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IMoedaParamRepository
{
    Task<List<MoedaParam>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Somente as moedas globais (AssessorId == null) — catálogo do admin.</summary>
    Task<List<MoedaParam>> GetGlobaisAsync(CancellationToken ct = default);
    /// <summary>Globais + as custom da assessoria informada.</summary>
    Task<List<MoedaParam>> GetGlobaisEDoAssessorAsync(Guid assessorId, CancellationToken ct = default);
    Task<MoedaParam?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(MoedaParam entity, CancellationToken ct = default);
    void Remove(MoedaParam entity);
}
