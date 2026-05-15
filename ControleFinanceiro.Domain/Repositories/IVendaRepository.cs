using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Repositories;

public interface IVendaRepository
{
    Task<IEnumerable<Venda>> GetAllAsync(DateTime? de, DateTime? ate, Guid? produtoId, StatusVenda? status, CancellationToken ct);
    Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Venda venda, CancellationToken ct);
    void Update(Venda venda);
    void Remove(Venda venda);
}
