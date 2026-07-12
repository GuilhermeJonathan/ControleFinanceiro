using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class VinculoAssessoriaRepository(AppDbContext db) : IVinculoAssessoriaRepository
{
    public Task<VinculoAssessoria?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.VinculosAssessoria.FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<VinculoAssessoria?> GetByCodigoAsync(string codigo, CancellationToken ct = default) =>
        db.VinculosAssessoria.FirstOrDefaultAsync(v => v.CodigoConvite == codigo.ToUpperInvariant(), ct);

    public Task<VinculoAssessoria?> GetVinculoAtivoAsync(Guid assessorId, Guid clienteId, CancellationToken ct = default) =>
        db.VinculosAssessoria.FirstOrDefaultAsync(v =>
            v.AssessorId == assessorId &&
            v.ClienteId == clienteId &&
            v.AceitoEm != null &&
            v.RevogadoEm == null, ct);

    public async Task<IEnumerable<VinculoAssessoria>> GetByAssessorAsync(Guid assessorId, CancellationToken ct = default) =>
        await db.VinculosAssessoria
            .Where(v => v.AssessorId == assessorId)
            .OrderByDescending(v => v.CriadoEm)
            .ToListAsync(ct);

    public Task<VinculoAssessoria?> GetByClienteAsync(Guid clienteId, CancellationToken ct = default) =>
        db.VinculosAssessoria.FirstOrDefaultAsync(v =>
            v.ClienteId == clienteId &&
            v.AceitoEm != null &&
            v.RevogadoEm == null, ct);

    public async Task AddAsync(VinculoAssessoria vinculo, CancellationToken ct = default) =>
        await db.VinculosAssessoria.AddAsync(vinculo, ct);
}
