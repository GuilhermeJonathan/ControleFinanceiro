using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class PatrimonioSnapshotRepository(AppDbContext db) : IPatrimonioSnapshotRepository
{
    public Task<PatrimonioSnapshot?> GetByUsuarioMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default) =>
        db.PatrimonioSnapshots.FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.Ano == ano && s.Mes == mes, ct);

    public async Task<IEnumerable<PatrimonioSnapshot>> GetByUsuarioAsync(Guid usuarioId, int meses, CancellationToken ct = default) =>
        await db.PatrimonioSnapshots
            .Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.Ano * 100 + s.Mes)
            .Take(meses)
            .ToListAsync(ct);

    public async Task AddAsync(PatrimonioSnapshot snapshot, CancellationToken ct = default) =>
        await db.PatrimonioSnapshots.AddAsync(snapshot, ct);

    public void Update(PatrimonioSnapshot snapshot) =>
        db.PatrimonioSnapshots.Update(snapshot);
}
