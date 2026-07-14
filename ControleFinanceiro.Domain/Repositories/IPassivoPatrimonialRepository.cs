using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IPassivoPatrimonialRepository
{
    Task<IEnumerable<PassivoPatrimonial>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<PassivoPatrimonial?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PassivoPatrimonial passivo, CancellationToken ct = default);
    void Update(PassivoPatrimonial passivo);
    void Remove(PassivoPatrimonial passivo);
}
