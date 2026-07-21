using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class EstruturaRepository(AppDbContext db) : IEstruturaRepository
{
    public Task<List<Estrutura>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        db.Estruturas.Where(e => e.UsuarioId == usuarioId).OrderBy(e => e.CriadoEm).ToListAsync(ct);

    public Task<Estrutura?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Estruturas.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Estrutura entity, CancellationToken ct = default) =>
        await db.Estruturas.AddAsync(entity, ct);

    public void Remove(Estrutura entity) => db.Estruturas.Remove(entity);

    public Task<List<ParticipacaoEstrutura>> GetParticipacoesByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        db.ParticipacoesEstrutura.Where(p => p.UsuarioId == usuarioId).ToListAsync(ct);

    public Task<ParticipacaoEstrutura?> GetParticipacaoByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ParticipacoesEstrutura.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddParticipacaoAsync(ParticipacaoEstrutura entity, CancellationToken ct = default) =>
        await db.ParticipacoesEstrutura.AddAsync(entity, ct);

    public void RemoveParticipacao(ParticipacaoEstrutura entity) =>
        db.ParticipacoesEstrutura.Remove(entity);
}
