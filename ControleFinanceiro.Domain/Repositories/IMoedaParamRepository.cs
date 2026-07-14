using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IMoedaParamRepository
{
    Task<List<MoedaParam>> GetAllAsync(CancellationToken ct = default);
    Task<MoedaParam?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(MoedaParam entity, CancellationToken ct = default);
    void Remove(MoedaParam entity);
}
