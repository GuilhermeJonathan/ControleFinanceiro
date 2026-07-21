using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ParametroOcultoRepository(AppDbContext db) : IParametroOcultoRepository
{
    public Task<List<int>> GetIdsOcultosAsync(Guid assessorId, TipoParametroCatalogo tipo, CancellationToken ct = default) =>
        db.ParametrosOcultos
          .Where(x => x.AssessorId == assessorId && x.Tipo == tipo)
          .Select(x => x.ParametroId)
          .ToListAsync(ct);

    public Task<ParametroOculto?> GetAsync(Guid assessorId, TipoParametroCatalogo tipo, int parametroId, CancellationToken ct = default) =>
        db.ParametrosOcultos.FirstOrDefaultAsync(
            x => x.AssessorId == assessorId && x.Tipo == tipo && x.ParametroId == parametroId, ct);

    public async Task AddAsync(ParametroOculto entity, CancellationToken ct = default) =>
        await db.ParametrosOcultos.AddAsync(entity, ct);

    public void Remove(ParametroOculto entity) =>
        db.ParametrosOcultos.Remove(entity);
}
