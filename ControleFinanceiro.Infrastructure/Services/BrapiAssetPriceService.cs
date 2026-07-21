using System.Text.Json;
using System.Text.Json.Serialization;
using ControleFinanceiro.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>
/// Consulta preços de ativos (ações, FIIs, ETFs, criptos) via brapi.dev.
/// A brapi.dev passou a exigir token — configure em "Brapi:Token" (env Brapi__Token).
/// Endpoint: https://brapi.dev/api/quote/{ticker}?token={token}
/// ATENÇÃO: o plano gratuito permite apenas 1 ativo por requisição → consultamos 1 a 1.
/// </summary>
public class BrapiAssetPriceService(
    HttpClient http,
    IConfiguration configuration,
    ILogger<BrapiAssetPriceService> logger) : IAssetPriceService
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly string? _token = configuration["Brapi:Token"];

    // Plano gratuito da brapi.dev aceita só 1 ativo por requisição.
    private const int LoteTamanho = 1;

    // Pausa entre requisições para não acionar rate limit (429).
    private static readonly TimeSpan DelayEntreRequisicoes = TimeSpan.FromSeconds(1);

    public async Task<Dictionary<string, decimal>> GetPricesAsync(
        IEnumerable<string> tickers, CancellationToken ct = default)
    {
        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var lista = tickers
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (lista.Count == 0) return result;

        if (string.IsNullOrWhiteSpace(_token))
        {
            logger.LogWarning("[AssetPrice] Token da brapi.dev não configurado (Brapi:Token). " +
                "A brapi.dev exige token — os preços não serão atualizados até configurar.");
            return result;
        }

        // Consulta 1 ativo por requisição (limite do plano gratuito da brapi.dev).
        for (int i = 0; i < lista.Count; i += LoteTamanho)
        {
            if (ct.IsCancellationRequested) break;

            var lote = lista.Skip(i).Take(LoteTamanho).ToList();
            await ProcessarLoteAsync(lote, result, ct);

            if (i + LoteTamanho < lista.Count)
                await Task.Delay(DelayEntreRequisicoes, ct);
        }

        logger.LogInformation("[AssetPrice] Preços obtidos: {tickers}", string.Join(", ", result.Keys));
        return result;
    }

    private async Task ProcessarLoteAsync(List<string> tickers, Dictionary<string, decimal> result, CancellationToken ct)
    {
        var url = $"https://brapi.dev/api/quote/{string.Join(",", tickers)}?fundamental=false&token={_token}";
        try
        {
            using var response = await http.GetAsync(url, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning("[AssetPrice] brapi.dev retornou 429 — aguardando 15s e tentando novamente...");
                await Task.Delay(TimeSpan.FromSeconds(15), ct);
                using var retry = await http.GetAsync(url, ct);
                if (!retry.IsSuccessStatusCode)
                {
                    logger.LogWarning("[AssetPrice] brapi.dev ainda indisponível ({status}). Lote pulado.", retry.StatusCode);
                    return;
                }
                ParseResponse(await retry.Content.ReadAsStringAsync(ct), result);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                var corpo = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning("[AssetPrice] brapi.dev retornou {status} para {tickers}: {corpo}",
                    response.StatusCode, string.Join(",", tickers), corpo);
                return;
            }

            ParseResponse(await response.Content.ReadAsStringAsync(ct), result);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[AssetPrice] Falha ao consultar brapi.dev para tickers: {tickers}.", string.Join(",", tickers));
        }
    }

    private void ParseResponse(string json, Dictionary<string, decimal> result)
    {
        try
        {
            var doc = JsonSerializer.Deserialize<BrapiResponseDto>(json, _jsonOpts);
            if (doc?.Results is null) return;

            foreach (var item in doc.Results)
            {
                if (string.IsNullOrWhiteSpace(item.Symbol)) continue;
                if (item.RegularMarketPrice is { } price && price > 0)
                    result[item.Symbol.ToUpperInvariant()] = price;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[AssetPrice] Falha ao parsear resposta da brapi.dev.");
        }
    }

    private sealed class BrapiResponseDto
    {
        [JsonPropertyName("results")]
        public List<BrapiQuoteDto>? Results { get; set; }
    }

    private sealed class BrapiQuoteDto
    {
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("regularMarketPrice")]
        public decimal? RegularMarketPrice { get; set; }
    }
}
