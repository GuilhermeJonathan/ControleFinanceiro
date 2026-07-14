using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ITipoAtivoParamRepository
{
    Task<List<TipoAtivoParam>> GetAllAsync(CancellationToken ct = default);
    Task<TipoAtivoParam?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(TipoAtivoParam entity, CancellationToken ct = default);
    void Remove(TipoAtivoParam entity);
}
