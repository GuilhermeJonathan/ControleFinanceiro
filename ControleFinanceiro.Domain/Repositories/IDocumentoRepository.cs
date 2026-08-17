using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IDocumentoRepository
{
    Task<List<Documento>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<List<Documento>> GetByAlvoAsync(Guid usuarioId, AlvoDocumento alvo, Guid? alvoId, CancellationToken ct = default);
    Task<Documento?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Documento documento, CancellationToken ct = default);
    void Remove(Documento documento);
}
