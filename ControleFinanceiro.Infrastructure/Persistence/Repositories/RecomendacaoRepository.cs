using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class RecomendacaoRepository(AppDbContext db) : IRecomendacaoRepository
{
    public Task<Recomendacao?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Recomendacoes.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Recomendacao>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default) =>
        await db.Recomendacoes
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(ct);

    public async Task<IEnumerable<Recomendacao>> GetByAssessorEClienteAsync(Guid assessorId, Guid clienteId, CancellationToken ct = default) =>
        await db.Recomendacoes
            .Where(r => r.AssessorId == assessorId && r.ClienteId == clienteId)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(ct);

    public async Task<IEnumerable<Recomendacao>> GetByAssessorAsync(Guid assessorId, CancellationToken ct = default) =>
        await db.Recomendacoes
            .Where(r => r.AssessorId == assessorId)
            .OrderByDescending(r => r.RespondidoEm ?? r.CriadoEm)
            .ToListAsync(ct);

    public Task<int> CountPendentesByClienteAsync(Guid clienteId, CancellationToken ct = default) =>
        db.Recomendacoes.CountAsync(r => r.ClienteId == clienteId && r.Status == StatusRecomendacao.Pendente, ct);

    public async Task AddAsync(Recomendacao recomendacao, CancellationToken ct = default) =>
        await db.Recomendacoes.AddAsync(recomendacao, ct);

    public void Remove(Recomendacao recomendacao) =>
        db.Recomendacoes.Remove(recomendacao);
}
