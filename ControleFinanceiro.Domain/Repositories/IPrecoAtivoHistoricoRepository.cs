using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IPrecoAtivoHistoricoRepository
{
    Task AddAsync(PrecoAtivoHistorico entity, CancellationToken ct = default);

    /// <summary>Histórico de um ticker, paginado, do mais recente ao mais antigo.</summary>
    Task<(List<PrecoAtivoHistorico> Items, int Total)> GetByTickerAsync(
        string ticker, int pagina = 1, int tamanhoPagina = 10, CancellationToken ct = default);
}
