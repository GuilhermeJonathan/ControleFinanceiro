namespace ControleFinanceiro.Application.Common.Interfaces;

public interface IAssetPriceService
{
    /// <summary>
    /// Busca o preço atual de ativos via ticker (ex: "PETR4", "MXRF11", "BTC").
    /// Retorna um dicionário onde a chave é o ticker e o valor é o preço atual em BRL.
    /// Tickers não encontrados ou com erro são omitidos.
    /// </summary>
    Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<string> tickers, CancellationToken ct = default);
}
