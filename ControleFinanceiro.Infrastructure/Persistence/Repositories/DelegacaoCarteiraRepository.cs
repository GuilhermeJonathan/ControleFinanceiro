using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class DelegacaoCarteiraRepository(AppDbContext db) : IDelegacaoCarteiraRepository
{
    public async Task<IEnumerable<DelegacaoCarteira>> GetByAssessorAsync(Guid assessorId, CancellationToken ct) =>
        await db.DelegacoesCarteira.Where(d => d.AssessorId == assessorId).OrderByDescending(d => d.DelegadoEm).ToListAsync(ct);

    public async Task<IEnumerable<DelegacaoCarteira>> GetByCorretorAsync(Guid corretorId, CancellationToken ct) =>
        await db.DelegacoesCarteira.Where(d => d.CorretorId == corretorId && d.RevogadoEm == null).ToListAsync(ct);

    public Task<DelegacaoCarteira?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.DelegacoesCarteira.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<DelegacaoCarteira?> GetAtivaAsync(Guid corretorId, Guid clienteId, CancellationToken ct) =>
        db.DelegacoesCarteira.FirstOrDefaultAsync(d =>
            d.CorretorId == corretorId && d.ClienteId == clienteId && d.RevogadoEm == null, ct);

    public Task<bool> ExisteAtivaAsync(Guid corretorId, Guid clienteId, CancellationToken ct) =>
        db.DelegacoesCarteira.AnyAsync(d =>
            d.CorretorId == corretorId && d.ClienteId == clienteId && d.RevogadoEm == null, ct);

    public async Task AddAsync(DelegacaoCarteira delegacao, CancellationToken ct) =>
        await db.DelegacoesCarteira.AddAsync(delegacao, ct);

    public void Update(DelegacaoCarteira delegacao) =>
        db.DelegacoesCarteira.Update(delegacao);
}
