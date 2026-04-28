using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IVinculoFamiliarRepository
{
    Task<VinculoFamiliar?> GetByCodigo(string codigo, CancellationToken ct);
    Task<Guid?> GetDonoIdAsync(Guid membroId, CancellationToken ct);
    Task<List<VinculoFamiliar>> GetByDonoAsync(Guid donoId, CancellationToken ct);
    Task AddAsync(VinculoFamiliar vinculo, CancellationToken ct);
    Task<VinculoFamiliar?> GetByIdAsync(Guid id, CancellationToken ct);
    void Remove(VinculoFamiliar vinculo);
}
