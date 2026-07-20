using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ICotacaoHistoricoRepository
{
    Task AddAsync(CotacaoHistorico entity, CancellationToken ct = default);

    /// <summary>Últimos registros de uma moeda paginados, do mais recente ao mais antigo.</summary>
    Task<(List<CotacaoHistorico> Items, int Total)> GetByMoedaAsync(
        string moedaCodigo, int pagina = 1, int tamanhoPagina = 10, CancellationToken ct = default);

    /// <summary>Cotações de todas as moedas entre duas datas.</summary>
    Task<List<CotacaoHistorico>> GetByPeriodoAsync(DateTime de, DateTime ate, CancellationToken ct = default);
}
