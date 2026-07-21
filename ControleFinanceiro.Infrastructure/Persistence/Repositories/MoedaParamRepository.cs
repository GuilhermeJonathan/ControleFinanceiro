using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class MoedaParamRepository(AppDbContext db) : IMoedaParamRepository
{
    public Task<List<MoedaParam>> GetAllAsync(CancellationToken ct = default) =>
        db.MoedasParam.OrderBy(x => x.Ordem).ToListAsync(ct);

    public Task<List<MoedaParam>> GetGlobaisAsync(CancellationToken ct = default) =>
        db.MoedasParam.Where(x => x.AssessorId == null).OrderBy(x => x.Ordem).ToListAsync(ct);

    public Task<List<MoedaParam>> GetGlobaisEDoAssessorAsync(Guid assessorId, CancellationToken ct = default) =>
        db.MoedasParam.Where(x => x.AssessorId == null || x.AssessorId == assessorId)
                      .OrderBy(x => x.Ordem).ToListAsync(ct);

    public Task<MoedaParam?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.MoedasParam.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(MoedaParam entity, CancellationToken ct = default) =>
        await db.MoedasParam.AddAsync(entity, ct);

    public void Remove(MoedaParam entity) =>
        db.MoedasParam.Remove(entity);
}
