using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IVinculoCorretorRepository
{
    Task<VinculoCorretor?> GetByCodigoAsync(string codigo, CancellationToken ct);
    Task<IEnumerable<VinculoCorretor>> GetByAssessorAsync(Guid assessorId, CancellationToken ct);
    Task<IEnumerable<VinculoCorretor>> GetByCorretorAsync(Guid corretorId, CancellationToken ct);
    Task<VinculoCorretor?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(VinculoCorretor vinculo, CancellationToken ct);
    void Update(VinculoCorretor vinculo);
}
