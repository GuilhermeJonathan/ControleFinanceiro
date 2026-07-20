using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class PlanoAcaoRepository(AppDbContext db) : IPlanoAcaoRepository
{
    // Etapas é owned → o EF já inclui a coleção automaticamente.
    public async Task<IEnumerable<PlanoAcao>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.PlanosAcao
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync(ct);

    public Task<PlanoAcao?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.PlanosAcao.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(PlanoAcao plano, CancellationToken ct = default) =>
        await db.PlanosAcao.AddAsync(plano, ct);

    public void Update(PlanoAcao plano) => db.PlanosAcao.Update(plano);

    public void Remove(PlanoAcao plano) => db.PlanosAcao.Remove(plano);
}
