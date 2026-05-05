using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ImovelRepository(AppDbContext db) : IImovelRepository
{
    public async Task<IEnumerable<Imovel>> GetAllAsync(Guid usuarioId, bool globalAccess = false, CancellationToken ct = default)
    {
        var q = db.Imoveis.Include(i => i.Fotos).Include(i => i.Comentarios);
        return globalAccess
            ? await q.OrderByDescending(i => i.Nota).ToListAsync(ct)
            : await q.Where(i => i.UsuarioId == usuarioId).OrderByDescending(i => i.Nota).ToListAsync(ct);
    }

    public Task<Imovel?> GetByIdAsync(Guid id, Guid usuarioId, bool globalAccess = false, CancellationToken ct = default)
    {
        var q = db.Imoveis.Include(i => i.Fotos).Include(i => i.Comentarios);
        return globalAccess
            ? q.FirstOrDefaultAsync(i => i.Id == id, ct)
            : q.FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId, ct);
    }

    public async Task AddAsync(Imovel imovel, CancellationToken ct = default) =>
        await db.Imoveis.AddAsync(imovel, ct);

    public void Update(Imovel imovel) => db.Imoveis.Update(imovel);

    public void Delete(Imovel imovel) => db.Imoveis.Remove(imovel);

    public async Task<ImovelFoto?> GetFotoAsync(Guid fotoId, Guid usuarioId, bool globalAccess = false, CancellationToken ct = default) =>
        await db.ImovelFotos
            .Include(f => f.Imovel)
            .FirstOrDefaultAsync(f => f.Id == fotoId && (globalAccess || f.Imovel.UsuarioId == usuarioId), ct);

    public async Task AddFotoAsync(ImovelFoto foto, CancellationToken ct = default) =>
        await db.ImovelFotos.AddAsync(foto, ct);

    public void DeleteFoto(ImovelFoto foto) => db.ImovelFotos.Remove(foto);

    public async Task<ImovelComentario?> GetComentarioAsync(Guid comentarioId, Guid usuarioId, bool globalAccess = false, CancellationToken ct = default) =>
        await db.ImovelComentarios
            .Include(c => c.Imovel)
            .FirstOrDefaultAsync(c => c.Id == comentarioId && (globalAccess || c.Imovel.UsuarioId == usuarioId), ct);

    public async Task AddComentarioAsync(ImovelComentario comentario, CancellationToken ct = default) =>
        await db.ImovelComentarios.AddAsync(comentario, ct);

    public void DeleteComentario(ImovelComentario comentario) => db.ImovelComentarios.Remove(comentario);
}
