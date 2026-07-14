using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class TipoAtivoParamRepository(AppDbContext db) : ITipoAtivoParamRepository
{
    public Task<List<TipoAtivoParam>> GetAllAsync(CancellationToken ct = default) =>
        db.TiposAtivoParam.OrderBy(x => x.Ordem).ToListAsync(ct);

    public Task<TipoAtivoParam?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.TiposAtivoParam.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TipoAtivoParam entity, CancellationToken ct = default) =>
        await db.TiposAtivoParam.AddAsync(entity, ct);

    public void Remove(TipoAtivoParam entity) =>
        db.TiposAtivoParam.Remove(entity);
}
