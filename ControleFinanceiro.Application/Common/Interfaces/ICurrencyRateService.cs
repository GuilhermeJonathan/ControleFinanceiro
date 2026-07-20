namespace ControleFinanceiro.Application.Common.Interfaces;

public interface ICurrencyRateService
{
    /// <summary>
    /// Busca as cotações de todas as moedas informadas em relação ao BRL.
    /// Retorna um dicionário onde a chave é o código da moeda (ex.: "USD") e o valor é a cotação em BRL.
    /// Moedas não encontradas ou com erro são omitidas do resultado.
    /// </summary>
    Task<Dictionary<string, decimal>> GetRatesVsBrlAsync(IEnumerable<string> moedas, CancellationToken ct = default);
}
