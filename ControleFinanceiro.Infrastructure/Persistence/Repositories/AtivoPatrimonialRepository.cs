using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class AtivoPatrimonialRepository(AppDbContext db) : IAtivoPatrimonialRepository
{
    public async Task<IEnumerable<AtivoPatrimonial>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.AtivosPatrimoniais
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.ValorAtual)
            .ToListAsync(ct);

    public Task<AtivoPatrimonial?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.AtivosPatrimoniais.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(AtivoPatrimonial ativo, CancellationToken ct = default) =>
        await db.AtivosPatrimoniais.AddAsync(ativo, ct);

    public void Update(AtivoPatrimonial ativo) => db.AtivosPatrimoniais.Update(ativo);

    public void Remove(AtivoPatrimonial ativo) => db.AtivosPatrimoniais.Remove(ativo);
}
