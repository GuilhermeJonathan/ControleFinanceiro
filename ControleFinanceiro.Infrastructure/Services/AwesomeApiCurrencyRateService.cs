using System.Text.Json;
using System.Text.Json.Serialization;
using ControleFinanceiro.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>
/// Consulta cotações em relação ao BRL via AwesomeAPI (gratuita, sem chave).
/// Endpoint: https://economia.awesomeapi.com.br/json/last/{pares}
/// Ex.: USD-BRL,EUR-BRL,GBP-BRL
/// </summary>
public class AwesomeApiCurrencyRateService(
    HttpClient http,
    ILogger<AwesomeApiCurrencyRateService> logger) : ICurrencyRateService
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<Dictionary<string, decimal>> GetRatesVsBrlAsync(
        IEnumerable<string> moedas, CancellationToken ct = default)
    {
        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var consultar = moedas
            .Where(m => !string.Equals(m, "BRL", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (consultar.Count == 0) return result;

        foreach (var moeda in consultar)
        {
            if (ct.IsCancellationRequested) break;

            var url = $"https://economia.awesomeapi.com.br/json/last/{moeda}-BRL";
            try
            {
                using var response = await http.GetAsync(url, ct);
                
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                var valor = ParseSingle(json, moeda);
                if (valor.HasValue) result[moeda] = valor.Value;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[CurrencyRate] Falha ao consultar {moeda}-BRL.", moeda);
            }

            // Pequeno delay entre moedas para não acionar rate limit
            if (consultar.IndexOf(moeda) < consultar.Count - 1)
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        logger.LogInformation("[CurrencyRate] Cotações obtidas: {codigos}", string.Join(", ", result.Keys));
        return result;
    }

    private decimal? ParseSingle(string json, string moedaCodigo)
    {
        try { return ParseBid(json); }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[CurrencyRate] Falha ao parsear resposta de {moeda}-BRL.", moedaCodigo);
            return null;
        }
    }

    /// <summary>Extrai o "bid" da resposta da AwesomeAPI (ex.: {"USDBRL":{"bid":"5.42"}}). Público p/ teste.</summary>
    public static decimal? ParseBid(string json)
    {
        var doc = JsonSerializer.Deserialize<Dictionary<string, AwesomeRateDto>>(json, _jsonOpts);
        var entry = doc?.Values.FirstOrDefault();
        if (entry is null) return null;
        return decimal.TryParse(entry.Bid, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var valor) ? valor : null;
    }

    private sealed class AwesomeRateDto
    {
        [JsonPropertyName("bid")]
        public string Bid { get; set; } = "0";
    }
}
