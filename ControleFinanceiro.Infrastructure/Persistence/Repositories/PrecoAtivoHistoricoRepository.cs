using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class PrecoAtivoHistoricoRepository(AppDbContext db) : IPrecoAtivoHistoricoRepository
{
    public async Task AddAsync(PrecoAtivoHistorico entity, CancellationToken ct = default) =>
        await db.PrecosAtivoHistorico.AddAsync(entity, ct);

    public async Task<(List<PrecoAtivoHistorico> Items, int Total)> GetByTickerAsync(
        string ticker, int pagina = 1, int tamanhoPagina = 10, CancellationToken ct = default)
    {
        var query = db.PrecosAtivoHistorico
            .Where(p => p.Ticker == ticker.ToUpperInvariant())
            .OrderByDescending(p => p.DataHora);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync(ct);
        return (items, total);
    }
}
