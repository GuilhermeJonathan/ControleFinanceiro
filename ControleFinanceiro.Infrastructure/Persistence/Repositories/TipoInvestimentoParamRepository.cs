using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class TipoInvestimentoParamRepository(AppDbContext db) : ITipoInvestimentoParamRepository
{
    public Task<List<TipoInvestimentoParam>> GetAllAsync(CancellationToken ct = default) =>
        db.TiposInvestimentoParam.OrderBy(x => x.Ordem).ToListAsync(ct);

    public Task<TipoInvestimentoParam?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.TiposInvestimentoParam.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TipoInvestimentoParam entity, CancellationToken ct = default) =>
        await db.TiposInvestimentoParam.AddAsync(entity, ct);

    public void Remove(TipoInvestimentoParam entity) =>
        db.TiposInvestimentoParam.Remove(entity);
}
