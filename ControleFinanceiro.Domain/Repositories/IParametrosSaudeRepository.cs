using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IParametrosSaudeRepository
{
    Task<ParametrosSaude?> GetByAssessorAsync(Guid assessorId, CancellationToken ct = default);
    Task AddAsync(ParametrosSaude parametros, CancellationToken ct = default);
    void Update(ParametrosSaude parametros);
}
