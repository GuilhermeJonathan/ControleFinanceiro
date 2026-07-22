using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class SubtipoInvestimentoParamRepository(AppDbContext db) : ISubtipoInvestimentoParamRepository
{
    public Task<List<SubtipoInvestimentoParam>> GetAllAsync(CancellationToken ct = default) =>
        db.SubtiposInvestimentoParam.OrderBy(s => s.TipoInvestimentoId).ThenBy(s => s.Ordem).ToListAsync(ct);

    public Task<List<SubtipoInvestimentoParam>> GetByTipoAsync(int tipoInvestimentoId, CancellationToken ct = default) =>
        db.SubtiposInvestimentoParam.Where(s => s.TipoInvestimentoId == tipoInvestimentoId)
            .OrderBy(s => s.Ordem).ToListAsync(ct);

    public Task<SubtipoInvestimentoParam?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.SubtiposInvestimentoParam.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(SubtipoInvestimentoParam entity, CancellationToken ct = default) =>
        await db.SubtiposInvestimentoParam.AddAsync(entity, ct);

    public void Remove(SubtipoInvestimentoParam entity) => db.SubtiposInvestimentoParam.Remove(entity);
}
