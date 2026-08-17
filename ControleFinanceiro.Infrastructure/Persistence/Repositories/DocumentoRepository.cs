using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class DocumentoRepository(AppDbContext db) : IDocumentoRepository
{
    public Task<List<Documento>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        db.Documentos.Where(d => d.UsuarioId == usuarioId).OrderByDescending(d => d.CriadoEm).ToListAsync(ct);

    public Task<List<Documento>> GetByAlvoAsync(Guid usuarioId, AlvoDocumento alvo, Guid? alvoId, CancellationToken ct = default) =>
        db.Documentos
            .Where(d => d.UsuarioId == usuarioId && d.Alvo == alvo && d.AlvoId == alvoId)
            .OrderByDescending(d => d.CriadoEm)
            .ToListAsync(ct);

    public Task<Documento?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Documentos.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task AddAsync(Documento documento, CancellationToken ct = default) =>
        await db.Documentos.AddAsync(documento, ct);

    public void Remove(Documento documento) => db.Documentos.Remove(documento);
}
