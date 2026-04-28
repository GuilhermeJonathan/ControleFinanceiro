using System.Net.Http.Headers;
using System.Text.Json;

namespace ControleFinanceiro.Api.WhatsApp;

public class WhatsAppSenderService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<WhatsAppSenderService> logger)
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private string PhoneNumberId => config["WhatsApp:PhoneNumberId"]
        ?? throw new InvalidOperationException("WhatsApp:PhoneNumberId não configurado.");

    private string AccessToken => config["WhatsApp:AccessToken"]
        ?? throw new InvalidOperationException("WhatsApp:AccessToken não configurado.");

    /// <summary>Envia uma mensagem de texto simples via WhatsApp.</summary>
    public async Task SendTextAsync(string to, string message, CancellationToken ct = default)
    {
        var payload = new
        {
            messaging_product = "whatsapp",
            to,
            type = "text",
            text = new { body = message }
        };

        await PostAsync(payload, ct);
    }

    private async Task PostAsync(object payload, CancellationToken ct)
    {
        var http = httpFactory.CreateClient("whatsapp");
        var url  = $"https://graph.facebook.com/v25.0/{PhoneNumberId}/messages";

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload, options: _json),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Meta API erro {Status}: {Body}", (int)response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }
    }
}
