using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class VinculoCorretorRepository(AppDbContext db) : IVinculoCorretorRepository
{
    public Task<VinculoCorretor?> GetByCodigoAsync(string codigo, CancellationToken ct) =>
        db.VinculosCorretor.FirstOrDefaultAsync(v => v.CodigoConvite == codigo.ToUpperInvariant(), ct);

    public async Task<IEnumerable<VinculoCorretor>> GetByAssessorAsync(Guid assessorId, CancellationToken ct) =>
        await db.VinculosCorretor.Where(v => v.AssessorId == assessorId).ToListAsync(ct);

    public async Task<IEnumerable<VinculoCorretor>> GetByCorretorAsync(Guid corretorId, CancellationToken ct) =>
        await db.VinculosCorretor.Where(v => v.CorretorId == corretorId).ToListAsync(ct);

    public Task<VinculoCorretor?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.VinculosCorretor.FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task AddAsync(VinculoCorretor vinculo, CancellationToken ct) =>
        await db.VinculosCorretor.AddAsync(vinculo, ct);

    public void Update(VinculoCorretor vinculo) =>
        db.VinculosCorretor.Update(vinculo);
}
