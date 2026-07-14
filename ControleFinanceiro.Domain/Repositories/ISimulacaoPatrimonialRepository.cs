using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ISimulacaoPatrimonialRepository
{
    Task<IEnumerable<SimulacaoPatrimonial>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<SimulacaoPatrimonial?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(SimulacaoPatrimonial simulacao, CancellationToken ct = default);
    void Update(SimulacaoPatrimonial simulacao);
    void Remove(SimulacaoPatrimonial simulacao);
}
