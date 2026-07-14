using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class PassivoPatrimonialRepository(AppDbContext db) : IPassivoPatrimonialRepository
{
    public async Task<IEnumerable<PassivoPatrimonial>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.PassivosPatrimoniais
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.Valor)
            .ToListAsync(ct);

    public Task<PassivoPatrimonial?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.PassivosPatrimoniais.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(PassivoPatrimonial passivo, CancellationToken ct = default) =>
        await db.PassivosPatrimoniais.AddAsync(passivo, ct);

    public void Update(PassivoPatrimonial passivo) => db.PassivosPatrimoniais.Update(passivo);

    public void Remove(PassivoPatrimonial passivo) => db.PassivosPatrimoniais.Remove(passivo);
}
