using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class MetaRepository(AppDbContext db) : IMetaRepository
{
    public Task<IEnumerable<Meta>> GetAllAsync(Guid usuarioId, CancellationToken ct) =>
        db.Metas.Where(m => m.UsuarioId == usuarioId)
            .OrderBy(m => m.Status).ThenBy(m => m.DataMeta).ThenBy(m => m.CriadoEm)
            .ToListAsync(ct).ContinueWith(t => (IEnumerable<Meta>)t.Result);

    public Task<Meta?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Metas.FindAsync([id], ct).AsTask();

    public async Task AddAsync(Meta meta, CancellationToken ct) =>
        await db.Metas.AddAsync(meta, ct);

    public void Update(Meta meta) => db.Metas.Update(meta);
    public void Remove(Meta meta) => db.Metas.Remove(meta);

    public async Task<List<Meta>> GetAllWithContribuicaoAsync(CancellationToken cancellationToken)
        => await db.Metas
            .Where(m => m.ContribuicaoMensalValor.HasValue && m.Status == StatusMeta.Ativa)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
