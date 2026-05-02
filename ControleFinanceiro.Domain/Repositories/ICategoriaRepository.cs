using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ICategoriaRepository
{
    Task<Categoria?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Categoria>> GetAllAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Categoria> Itens, int TotalCount)> GetPagedAsync(Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Categoria categoria, CancellationToken cancellationToken = default);
    void Update(Categoria categoria);
    void Delete(Categoria categoria);
}
