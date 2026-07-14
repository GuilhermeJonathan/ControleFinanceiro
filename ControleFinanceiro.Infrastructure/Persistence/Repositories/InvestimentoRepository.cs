using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class InvestimentoRepository(AppDbContext db) : IInvestimentoRepository
{
    public async Task<IEnumerable<Investimento>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.Investimentos
            .Where(i => i.UsuarioId == usuarioId)
            .OrderByDescending(i => i.ValorAtual)
            .ToListAsync(ct);

    public Task<Investimento?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Investimentos.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task AddAsync(Investimento investimento, CancellationToken ct = default) =>
        await db.Investimentos.AddAsync(investimento, ct);

    public void Update(Investimento investimento) => db.Investimentos.Update(investimento);

    public void Remove(Investimento investimento) => db.Investimentos.Remove(investimento);
}
