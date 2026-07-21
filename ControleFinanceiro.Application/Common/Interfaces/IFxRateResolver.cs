namespace ControleFinanceiro.Application.Common.Interfaces;

/// <summary>
/// Resolve a tabela de câmbio (código → CotacaoBRL) efetiva para o usuário atual:
/// moedas globais NÃO ocultas + moedas custom da assessoria dona (custom sobrescreve global de mesmo código).
/// Centraliza a conversão para BRL em todos os handlers de patrimônio/investimentos/projeções.
/// </summary>
public interface IFxRateResolver
{
    Task<IReadOnlyDictionary<string, decimal>> GetRatesAsync(CancellationToken ct = default);
}
