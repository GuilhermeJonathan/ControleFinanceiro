using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IEstruturaRepository
{
    Task<List<Estrutura>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Estrutura?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Estrutura entity, CancellationToken ct = default);
    void Remove(Estrutura entity);

    // Participações (arestas do grafo do mesmo usuário)
    Task<List<ParticipacaoEstrutura>> GetParticipacoesByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<ParticipacaoEstrutura?> GetParticipacaoByIdAsync(Guid id, CancellationToken ct = default);
    Task AddParticipacaoAsync(ParticipacaoEstrutura entity, CancellationToken ct = default);
    void RemoveParticipacao(ParticipacaoEstrutura entity);
}
