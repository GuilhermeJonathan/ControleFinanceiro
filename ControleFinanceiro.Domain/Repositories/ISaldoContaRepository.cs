using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ISaldoContaRepository
{
    Task<SaldoConta?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<SaldoConta?> GetByBancoAsync(string banco, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SaldoConta>> GetAllAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task AddAsync(SaldoConta saldo, CancellationToken cancellationToken = default);
    void Update(SaldoConta saldo);
    void Delete(SaldoConta saldo);
}
