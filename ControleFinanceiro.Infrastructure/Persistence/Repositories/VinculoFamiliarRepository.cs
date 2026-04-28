using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class VinculoFamiliarRepository(AppDbContext db) : IVinculoFamiliarRepository
{
    public Task<VinculoFamiliar?> GetByCodigo(string codigo, CancellationToken ct) =>
        db.VinculosFamiliares.FirstOrDefaultAsync(v => v.CodigoConvite == codigo.ToUpperInvariant(), ct);

    public Task<Guid?> GetDonoIdAsync(Guid membroId, CancellationToken ct) =>
        db.VinculosFamiliares
            .Where(v => v.MembroId == membroId && v.AceitoEm != null)
            .Select(v => (Guid?)v.DonoId)
            .FirstOrDefaultAsync(ct);

    public Task<List<VinculoFamiliar>> GetByDonoAsync(Guid donoId, CancellationToken ct) =>
        db.VinculosFamiliares.Where(v => v.DonoId == donoId).ToListAsync(ct);

    public async Task AddAsync(VinculoFamiliar vinculo, CancellationToken ct) =>
        await db.VinculosFamiliares.AddAsync(vinculo, ct);

    public Task<VinculoFamiliar?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.VinculosFamiliares.FindAsync([id], ct).AsTask();

    public void Remove(VinculoFamiliar vinculo) => db.VinculosFamiliares.Remove(vinculo);
}
