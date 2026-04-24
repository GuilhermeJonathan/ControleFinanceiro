using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ICategoriaRepository
{
    Task<Categoria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Categoria>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Categoria categoria, CancellationToken cancellationToken = default);
    void Delete(Categoria categoria);
}
