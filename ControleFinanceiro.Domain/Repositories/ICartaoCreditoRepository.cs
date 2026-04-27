using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ICartaoCreditoRepository
{
    Task<CartaoCredito?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<CartaoCredito?> GetByIdWithParcelasAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CartaoCredito>> GetAllWithParcelasAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task AddAsync(CartaoCredito cartao, CancellationToken cancellationToken = default);
    void Update(CartaoCredito cartao);
    void Delete(CartaoCredito cartao);
}
