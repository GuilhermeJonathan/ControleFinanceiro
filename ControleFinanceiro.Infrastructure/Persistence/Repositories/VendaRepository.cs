using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class VendaRepository(AppDbContext db) : IVendaRepository
{
    public Task<IEnumerable<Venda>> GetAllAsync(DateTime? de, DateTime? ate,
        Guid? produtoId, StatusVenda? status, CancellationToken ct)
    {
        var query = db.Vendas.AsQueryable();

        if (de.HasValue)
            query = query.Where(v => v.Data >= de.Value);
        if (ate.HasValue)
            query = query.Where(v => v.Data <= ate.Value);
        if (produtoId.HasValue)
            query = query.Where(v => v.ProdutoId == produtoId.Value);
        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);

        return query.OrderByDescending(v => v.Data)
            .ToListAsync(ct).ContinueWith(t => (IEnumerable<Venda>)t.Result);
    }

    public Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Vendas.FindAsync([id], ct).AsTask();

    public async Task AddAsync(Venda venda, CancellationToken ct) =>
        await db.Vendas.AddAsync(venda, ct);

    public void Update(Venda venda) => db.Vendas.Update(venda);
    public void Remove(Venda venda) => db.Vendas.Remove(venda);
}
