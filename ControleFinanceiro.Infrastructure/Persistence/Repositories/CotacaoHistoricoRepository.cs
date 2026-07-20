using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class CotacaoHistoricoRepository(AppDbContext db) : ICotacaoHistoricoRepository
{
    public async Task AddAsync(CotacaoHistorico entity, CancellationToken ct = default) =>
        await db.CotacoesHistorico.AddAsync(entity, ct);

    public async Task<(List<CotacaoHistorico> Items, int Total)> GetByMoedaAsync(
        string moedaCodigo, int pagina = 1, int tamanhoPagina = 10, CancellationToken ct = default)
    {
        var query = db.CotacoesHistorico
            .Where(c => c.MoedaCodigo == moedaCodigo.ToUpperInvariant())
            .OrderByDescending(c => c.DataHora);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<List<CotacaoHistorico>> GetByPeriodoAsync(
        DateTime de, DateTime ate, CancellationToken ct = default) =>
        db.CotacoesHistorico
          .Where(c => c.DataHora >= de && c.DataHora <= ate)
          .OrderBy(c => c.MoedaCodigo)
          .ThenByDescending(c => c.DataHora)
          .ToListAsync(ct);
}
