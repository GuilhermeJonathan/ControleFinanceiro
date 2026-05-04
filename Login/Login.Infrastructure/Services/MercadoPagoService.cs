using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Login.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Login.Infrastructure.Services;

public class MercadoPagoService : IMercadoPagoService
{
    private const string BaseUrl = "https://api.mercadopago.com";

    private readonly HttpClient _http;
    private readonly string _accessToken;
    private readonly string _webhookSecret;
    private readonly string _backUrl;
    private readonly ILogger<MercadoPagoService> _logger;

    // Preços dos planos em BRL
    private const decimal PriceMonthly = 4.90m;
    private const decimal PriceAnnual  = 39.90m;

    public MercadoPagoService(
        HttpClient http,
        IConfiguration configuration,
        ILogger<MercadoPagoService> logger)
    {
        _http         = http;
        _logger       = logger;
        _accessToken  = configuration["MercadoPago:AccessToken"]
            ?? throw new InvalidOperationException("MercadoPago:AccessToken não configurado.");
        _webhookSecret = configuration["MercadoPago:WebhookSecret"] ?? string.Empty;
        _backUrl       = configuration["MercadoPago:BackUrl"] ?? "https://app.findog.com.br";

        _http.BaseAddress = new Uri(BaseUrl);
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<MpCheckoutResult> CreateSubscriptionAsync(
        string userEmail,
        Guid userId,
        string planId,
        CancellationToken cancellationToken = default)
    {
        var isAnnual = planId == "anual";
        var amount   = isAnnual ? PriceAnnual : PriceMonthly;
        var reason   = isAnnual
            ? "Plano Anual · Meu FinDog"
            : "Plano Mensal · Meu FinDog";

        var payload = new
        {
            reason,
            external_reference = userId.ToString(),
            payer_email        = userEmail,
            auto_recurring = new
            {
                frequency      = 1,
                frequency_type = isAnnual ? "years" : "months",
                transaction_amount = amount,
                currency_id    = "BRL",
            },
            back_url = _backUrl,
            status   = "pending",
        };

        var json    = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("/preapproval", content, cancellationToken);
        var body     = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erro ao criar preapproval no MP: {Status} {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Mercado Pago retornou {(int)response.StatusCode}: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var subscriptionId = root.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Mercado Pago não retornou 'id'.");
        var initPoint = root.GetProperty("init_point").GetString()
            ?? throw new InvalidOperationException("Mercado Pago não retornou 'init_point'.");

        _logger.LogInformation("Preapproval criado: {Id} para usuário {UserId}", subscriptionId, userId);

        return new MpCheckoutResult(subscriptionId, initPoint);
    }

    public async Task<MpSubscriptionDetail?> GetSubscriptionAsync(
        string mpSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"/preapproval/{mpSubscriptionId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("MP retornou {Status} ao buscar preapproval {Id}",
                response.StatusCode, mpSubscriptionId);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var id     = root.GetProperty("id").GetString() ?? mpSubscriptionId;
        var status = root.TryGetProperty("status", out var s) ? s.GetString() ?? "unknown" : "unknown";
        var exRef  = root.TryGetProperty("external_reference", out var er) ? er.GetString() ?? "" : "";
        var amount = root.TryGetProperty("auto_recurring", out var ar) &&
                     ar.TryGetProperty("transaction_amount", out var ta)
                     ? ta.GetDecimal() : 0m;

        // Tenta obter o ID do último pagamento (campo summarized.last_charged_date_* não existe;
        // o campo correto é summarized.last_charged ou payment_method_id — usar external_id se disponível)
        string? lastPaymentId = null;
        if (root.TryGetProperty("summarized", out var sum) &&
            sum.TryGetProperty("last_charged_date", out var lcd))
        {
            lastPaymentId = lcd.GetString();
        }

        return new MpSubscriptionDetail(id, status, exRef, lastPaymentId, amount);
    }

    public bool ValidateWebhookSignature(
        string xSignature,
        string xRequestId,
        string dataId)
    {
        if (string.IsNullOrEmpty(_webhookSecret)) return true; // sem segredo configurado: aceita tudo (dev)

        // Formato: ts=TIMESTAMP,v1=HASH
        var parts = xSignature.Split(',');
        string? ts = null;
        string? v1 = null;

        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            if (kv[0] == "ts") ts = kv[1];
            if (kv[0] == "v1") v1 = kv[1];
        }

        if (ts is null || v1 is null) return false;

        // Manifesto conforme documentação do MP
        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts}";
        var secretBytes = Encoding.UTF8.GetBytes(_webhookSecret);
        var messageBytes = Encoding.UTF8.GetBytes(manifest);

        var hash = HMACSHA256.HashData(secretBytes, messageBytes);
        var computed = Convert.ToHexString(hash).ToLower();

        return computed == v1.ToLower();
    }
}
