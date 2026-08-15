using System.Text.Json;
using ControleFinanceiro.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>
/// Consulta preços de ativos negociados no exterior via Yahoo Finance (grátis, sem token).
/// Endpoint: https://query1.finance.yahoo.com/v8/finance/chart/{ticker}
/// Ex.: "VWRA.L" (Vanguard FTSE All-World, bolsa de Londres). Lê meta.regularMarketPrice.
/// Uma requisição por ticker (o endpoint chart aceita 1 símbolo por chamada).
/// ATENÇÃO: o preço vem na MOEDA DE NEGOCIAÇÃO do ativo (ex.: VWRA.L em USD) — o
/// investimento deve estar cadastrado na mesma moeda para a conversão a BRL ficar correta.
/// </summary>
public class YahooAssetPriceService(
    HttpClient http,
    ILogger<YahooAssetPriceService> logger) : IExteriorAssetPriceService
{
    // Pequena pausa entre chamadas para não parecer abuso ao Yahoo.
    private static readonly TimeSpan DelayEntreRequisicoes = TimeSpan.FromMilliseconds(400);

    public async Task<Dictionary<string, decimal>> GetPricesAsync(
        IEnumerable<string> tickers, CancellationToken ct = default)
    {
        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var lista = tickers
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        for (int i = 0; i < lista.Count; i++)
        {
            if (ct.IsCancellationRequested) break;
            await ConsultarAsync(lista[i], result, ct);
            if (i + 1 < lista.Count) await Task.Delay(DelayEntreRequisicoes, ct);
        }

        logger.LogInformation("[AssetPrice/Yahoo] Preços obtidos: {tickers}", string.Join(", ", result.Keys));
        return result;
    }

    private async Task ConsultarAsync(string ticker, Dictionary<string, decimal> result, CancellationToken ct)
    {
        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(ticker)}";
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            // Yahoo rejeita requisições sem User-Agent de navegador.
            req.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            using var resp = await http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("[AssetPrice/Yahoo] {status} para {ticker}.", resp.StatusCode, ticker);
                return;
            }

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            if (!doc.RootElement.TryGetProperty("chart", out var chart)) return;
            if (!chart.TryGetProperty("result", out var results) || results.ValueKind != JsonValueKind.Array
                || results.GetArrayLength() == 0) return;

            var meta = results[0].GetProperty("meta");
            if (meta.TryGetProperty("regularMarketPrice", out var precoEl)
                && precoEl.ValueKind == JsonValueKind.Number
                && precoEl.TryGetDecimal(out var preco) && preco > 0)
            {
                result[ticker] = preco;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[AssetPrice/Yahoo] Falha ao consultar {ticker}.", ticker);
        }
    }
}
