using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ProdutoRepository(AppDbContext db) : IProdutoRepository
{
    public Task<IEnumerable<Produto>> GetAllAsync(Guid usuarioId, CancellationToken ct) =>
        db.Produtos.Where(p => p.UsuarioId == usuarioId)
            .OrderBy(p => p.Nome)
            .ToListAsync(ct).ContinueWith(t => (IEnumerable<Produto>)t.Result);

    public Task<Produto?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Produtos.FindAsync([id], ct).AsTask();

    public Task<Produto?> GetByNomeAsync(Guid usuarioId, string nome, CancellationToken ct) =>
        db.Produtos.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.Nome == nome, ct);

    public async Task AddAsync(Produto produto, CancellationToken ct) =>
        await db.Produtos.AddAsync(produto, ct);

    public void Update(Produto produto) => db.Produtos.Update(produto);
    public void Remove(Produto produto) => db.Produtos.Remove(produto);
}
