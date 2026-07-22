using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class IndicadoresSucessaoRepository(AppDbContext db) : IIndicadoresSucessaoRepository
{
    public Task<IndicadoresSucessao?> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        db.IndicadoresSucessao.FirstOrDefaultAsync(i => i.UsuarioId == usuarioId, ct);

    public async Task AddAsync(IndicadoresSucessao entity, CancellationToken ct = default) =>
        await db.IndicadoresSucessao.AddAsync(entity, ct);
}
