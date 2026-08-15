using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IConsultoriaConfigRepository
{
    Task<ConsultoriaConfig?> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    /// <summary>Busca a consultoria pelo slug (rota do login whitelabel). null = não encontrada.</summary>
    Task<ConsultoriaConfig?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(ConsultoriaConfig config, CancellationToken ct = default);
    void Update(ConsultoriaConfig config);
}
