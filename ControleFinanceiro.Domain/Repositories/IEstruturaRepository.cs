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

    // Beneficiários (do cliente)
    Task<List<Beneficiario>> GetBeneficiariosByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Beneficiario?> GetBeneficiarioByIdAsync(Guid id, CancellationToken ct = default);
    Task AddBeneficiarioAsync(Beneficiario entity, CancellationToken ct = default);
    void RemoveBeneficiario(Beneficiario entity);

    // Distribuições (do cliente)
    Task<List<Distribuicao>> GetDistribuicoesByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Distribuicao?> GetDistribuicaoByIdAsync(Guid id, CancellationToken ct = default);
    Task AddDistribuicaoAsync(Distribuicao entity, CancellationToken ct = default);
    void RemoveDistribuicao(Distribuicao entity);
}
