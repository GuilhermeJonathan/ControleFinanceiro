using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class CategoriaRepository(AppDbContext context) : ICategoriaRepository
{
    public async Task<Categoria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Categorias.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<Categoria>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Categorias.OrderBy(c => c.Nome).ToListAsync(cancellationToken);

    public async Task AddAsync(Categoria categoria, CancellationToken cancellationToken = default)
        => await context.Categorias.AddAsync(categoria, cancellationToken);

    public void Delete(Categoria categoria) => context.Categorias.Remove(categoria);
}
