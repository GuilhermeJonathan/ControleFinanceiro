using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IParametroOcultoRepository
{
    /// <summary>Ids dos parâmetros globais que a assessoria ocultou para uma categoria.</summary>
    Task<List<int>> GetIdsOcultosAsync(Guid assessorId, TipoParametroCatalogo tipo, CancellationToken ct = default);
    Task<ParametroOculto?> GetAsync(Guid assessorId, TipoParametroCatalogo tipo, int parametroId, CancellationToken ct = default);
    Task AddAsync(ParametroOculto entity, CancellationToken ct = default);
    void Remove(ParametroOculto entity);
}
