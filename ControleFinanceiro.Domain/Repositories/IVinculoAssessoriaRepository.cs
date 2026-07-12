using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IVinculoAssessoriaRepository
{
    Task<VinculoAssessoria?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VinculoAssessoria?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    /// <summary>Vínculo aceito e não revogado entre assessor e cliente — o guard central da feature.</summary>
    Task<VinculoAssessoria?> GetVinculoAtivoAsync(Guid assessorId, Guid clienteId, CancellationToken ct = default);
    /// <summary>Todos os vínculos do assessor (pendentes, ativos e revogados).</summary>
    Task<IEnumerable<VinculoAssessoria>> GetByAssessorAsync(Guid assessorId, CancellationToken ct = default);
    /// <summary>Vínculo ativo do cliente (um cliente tem no máximo um assessor ativo).</summary>
    Task<VinculoAssessoria?> GetByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task AddAsync(VinculoAssessoria vinculo, CancellationToken ct = default);
}
