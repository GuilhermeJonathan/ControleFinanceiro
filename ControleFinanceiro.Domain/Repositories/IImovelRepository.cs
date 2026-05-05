using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IImovelRepository
{
    Task<IEnumerable<Imovel>> GetAllAsync(Guid usuarioId, bool globalAccess = false, CancellationToken ct = default);
    Task<Imovel?> GetByIdAsync(Guid id, Guid usuarioId, bool globalAccess = false, CancellationToken ct = default);
    Task AddAsync(Imovel imovel, CancellationToken ct = default);
    void Update(Imovel imovel);
    void Delete(Imovel imovel);
    Task<ImovelFoto?> GetFotoAsync(Guid fotoId, Guid usuarioId, bool globalAccess = false, CancellationToken ct = default);
    Task AddFotoAsync(ImovelFoto foto, CancellationToken ct = default);
    void DeleteFoto(ImovelFoto foto);

    Task<ImovelComentario?> GetComentarioAsync(Guid comentarioId, Guid usuarioId, bool globalAccess = false, CancellationToken ct = default);
    Task AddComentarioAsync(ImovelComentario comentario, CancellationToken ct = default);
    void DeleteComentario(ImovelComentario comentario);
}
