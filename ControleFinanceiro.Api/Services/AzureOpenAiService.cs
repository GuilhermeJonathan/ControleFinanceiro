using ControleFinanceiro.Application.Common.Interfaces;
using System.Text.Json;

namespace ControleFinanceiro.Api.Services;

/// <summary>
/// Implementação de <see cref="IAiService"/> usando Azure OpenAI (chat completions).
/// Reutiliza o HttpClient nomeado "openai" e as chaves AzureOpenAI:* do appsettings.
/// </summary>
public class AzureOpenAiService(
    IHttpClientFactory httpFactory,
    IConfiguration configuration,
    ILogger<AzureOpenAiService> logger) : IAiService
{
    private const string ApiVersion = "2024-06-01";

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private string Endpoint   => (configuration["AzureOpenAI:Endpoint"] ?? "").TrimEnd('/');
    private string ApiKey     => configuration["AzureOpenAI:ApiKey"]
        ?? throw new InvalidOperationException("AzureOpenAI:ApiKey não configurado.");
    private string Deployment => configuration["AzureOpenAI:VisionDeployment"] ?? "gpt-4o-mini";

    public async Task<string> ChatAsync(
        string systemPrompt,
        string userMessage,
        int   maxTokens   = 800,
        float temperature = 0.3f,
        CancellationToken cancellationToken = default)
    {
        var url = $"{Endpoint}/openai/deployments/{Deployment}/chat/completions?api-version={ApiVersion}";

        var payload = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userMessage  },
            },
            max_completion_tokens = maxTokens,
            temperature           = temperature,
        };

        var http = httpFactory.CreateClient("openai");
        var req  = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            })
        };
        req.Headers.Add("api-key", ApiKey);

        var res  = await http.SendAsync(req, cancellationToken);
        var body = await res.Content.ReadAsStringAsync(cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("AzureOpenAI ChatAsync erro {Status} | URL={Url} | Body={Body}",
                (int)res.StatusCode, url, body);
            throw new InvalidOperationException(
                $"AzureOpenAI retornou {(int)res.StatusCode}: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        logger.LogDebug("IAiService resposta: {Chars} chars", content.Length);
        return content;
    }
}
