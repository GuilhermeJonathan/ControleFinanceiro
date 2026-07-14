using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IAtivoPatrimonialRepository
{
    Task<IEnumerable<AtivoPatrimonial>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<AtivoPatrimonial?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AtivoPatrimonial ativo, CancellationToken ct = default);
    void Update(AtivoPatrimonial ativo);
    void Remove(AtivoPatrimonial ativo);
}
