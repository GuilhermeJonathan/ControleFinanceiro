using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IConsultoriaConfigRepository
{
    Task<ConsultoriaConfig?> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AddAsync(ConsultoriaConfig config, CancellationToken ct = default);
    void Update(ConsultoriaConfig config);
}
