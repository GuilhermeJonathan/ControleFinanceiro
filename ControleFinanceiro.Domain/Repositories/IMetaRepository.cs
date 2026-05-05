using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IMetaRepository
{
    Task<IEnumerable<Meta>> GetAllAsync(Guid usuarioId, CancellationToken ct);
    Task<Meta?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Meta meta, CancellationToken ct);
    void Update(Meta meta);
    void Remove(Meta meta);
    Task<List<Meta>> GetAllWithContribuicaoAsync(CancellationToken cancellationToken);
}
