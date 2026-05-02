using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class CategoriaRepository(AppDbContext context) : ICategoriaRepository
{
    public async Task<Categoria?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<Categoria>> GetAllAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Categorias.Where(c => c.UsuarioId == usuarioId).OrderBy(c => c.Nome).ToListAsync(cancellationToken);

    public async Task<(IEnumerable<Categoria> Itens, int TotalCount)> GetPagedAsync(Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Categorias.Where(c => c.UsuarioId == usuarioId).OrderBy(c => c.Nome);
        var total = await query.CountAsync(cancellationToken);
        var itens = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (itens, total);
    }

    public async Task AddAsync(Categoria categoria, CancellationToken cancellationToken = default)
        => await context.Categorias.AddAsync(categoria, cancellationToken);

    public void Update(Categoria categoria) => context.Categorias.Update(categoria);
    public void Delete(Categoria categoria) => context.Categorias.Remove(categoria);
}
