using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> GetAllAsync(Guid usuarioId, CancellationToken ct);
    Task<Produto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Produto?> GetByNomeAsync(Guid usuarioId, string nome, CancellationToken ct);
    Task AddAsync(Produto produto, CancellationToken ct);
    void Update(Produto produto);
    void Remove(Produto produto);
}
