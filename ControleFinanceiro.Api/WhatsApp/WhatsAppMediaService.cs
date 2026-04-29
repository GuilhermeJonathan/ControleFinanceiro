using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ControleFinanceiro.Api.WhatsApp;

/// <summary>
/// Baixa mídias da Meta e usa OpenAI (Whisper + GPT-4o) para
/// converter áudio/imagem em texto processável pelo WhatsAppMessageParser.
/// </summary>
public class WhatsAppMediaService(
    IHttpClientFactory httpFactory,
    IConfiguration config,
    ILogger<WhatsAppMediaService> logger)
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private string AccessToken => config["WhatsApp:AccessToken"]
        ?? throw new InvalidOperationException("WhatsApp:AccessToken não configurado.");

    // ── Azure OpenAI ──────────────────────────────────────────────────────────
    private string AzureEndpoint => (config["AzureOpenAI:Endpoint"] ?? "").TrimEnd('/');
    private string AzureApiKey   => config["AzureOpenAI:ApiKey"]
        ?? throw new InvalidOperationException("AzureOpenAI:ApiKey não configurado.");
    private string WhisperDeployment => config["AzureOpenAI:WhisperDeployment"] ?? "whisper";
    private string VisionDeployment  => config["AzureOpenAI:VisionDeployment"]  ?? "gpt-4o-mini";
    private const string ApiVersion  = "2024-06-01";

    // ── Download de mídia da Meta ─────────────────────────────────────────────

    public async Task<(byte[] Data, string MimeType)> DownloadMediaAsync(
        string mediaId, CancellationToken ct)
    {
        var http = httpFactory.CreateClient("whatsapp");

        // 1. Busca a URL temporária da mídia
        var infoReq = new HttpRequestMessage(
            HttpMethod.Get, $"https://graph.facebook.com/v25.0/{mediaId}");
        infoReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var infoRes = await http.SendAsync(infoReq, ct);
        infoRes.EnsureSuccessStatusCode();

        var info = JsonSerializer.Deserialize<MediaUrlResponse>(
            await infoRes.Content.ReadAsStringAsync(ct), _jsonOpts)
            ?? throw new InvalidOperationException("Resposta inválida ao buscar URL da mídia.");

        if (string.IsNullOrEmpty(info.Url))
            throw new InvalidOperationException("Meta não retornou URL para a mídia.");

        // 2. Baixa o arquivo usando o Bearer token
        var dlReq = new HttpRequestMessage(HttpMethod.Get, info.Url);
        dlReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var dlRes = await http.SendAsync(dlReq, ct);
        dlRes.EnsureSuccessStatusCode();

        var bytes = await dlRes.Content.ReadAsByteArrayAsync(ct);
        logger.LogInformation("Mídia {Id} baixada: {Bytes} bytes, tipo={Mime}",
            mediaId, bytes.Length, info.MimeType);

        return (bytes, info.MimeType ?? "application/octet-stream");
    }

    // ── Transcrição de áudio (Whisper) ────────────────────────────────────────

    /// <summary>Transcreve uma mensagem de voz do WhatsApp e retorna o texto em português.</summary>
    public async Task<string> TranscribeAudioAsync(string mediaId, CancellationToken ct)
    {
        var (data, mimeType) = await DownloadMediaAsync(mediaId, ct);

        // WhatsApp envia áudio como audio/ogg (codec opus) na maioria das vezes
        var ext = mimeType switch
        {
            var m when m.Contains("ogg")  => "ogg",
            var m when m.Contains("mp4")  => "mp4",
            var m when m.Contains("mpeg") => "mp3",
            var m when m.Contains("webm") => "webm",
            _                             => "ogg",
        };

        using var form = new MultipartFormDataContent();

        var audioContent = new ByteArrayContent(data);
        audioContent.Headers.ContentType = new(mimeType);
        form.Add(audioContent, "file", $"audio.{ext}");
        form.Add(new StringContent("pt"), "language"); // força português

        // Azure OpenAI: POST {endpoint}/openai/deployments/{deployment}/audio/transcriptions?api-version=...
        var url = $"{AzureEndpoint}/openai/deployments/{WhisperDeployment}/audio/transcriptions?api-version={ApiVersion}";

        var http = httpFactory.CreateClient("openai");
        var req  = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
        req.Headers.Add("api-key", AzureApiKey); // Azure usa api-key, não Authorization Bearer

        var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var result = JsonSerializer.Deserialize<TranscriptionResponse>(
            await res.Content.ReadAsStringAsync(ct), _jsonOpts);

        var text = result?.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            throw new InvalidOperationException("Whisper não retornou transcrição.");

        logger.LogInformation("Áudio {Id} transcrito: {Text}", mediaId, text);
        return text;
    }

    // ── Extração de imagem (GPT-4o Vision) ───────────────────────────────────

    /// <summary>
    /// Analisa uma foto de cupom/recibo via GPT-4o e retorna uma linha de texto
    /// no formato reconhecido pelo WhatsAppMessageParser, ex: "Supermercado 245,80".
    /// </summary>
    public async Task<string> ExtractFromImageAsync(
        string mediaId, string? caption, CancellationToken ct)
    {
        var (data, mimeType) = await DownloadMediaAsync(mediaId, ct);
        var base64 = Convert.ToBase64String(data);

        const string systemPrompt = """
            Você é um assistente financeiro pessoal. O usuário enviou uma foto de
            cupom fiscal, comprovante, nota, recibo ou tela de pagamento.
            Extraia as informações e responda APENAS com uma linha de texto simples:
            "<descrição breve do estabelecimento ou item> <valor total em reais>"
            Use vírgula como separador decimal, ex: 245,80
            Exemplos de respostas válidas:
            - Supermercado Atacadão 245,80
            - Restaurante Pizza 67,50
            - Posto Shell gasolina 180,00
            - iFood delivery 38,90
            Se não conseguir identificar claramente valor e descrição, responda: ERRO
            NÃO inclua explicações, prefixos nem pontuação extra.
            """;

        var userText = string.IsNullOrWhiteSpace(caption)
            ? "Extraia o lançamento desta imagem."
            : $"Legenda do usuário: \"{caption}\". Extraia o lançamento desta imagem.";

        var payload = new
        {
            model      = "gpt-4o-mini",
            max_tokens = 80,
            messages   = new object[]
            {
                new { role = "system", content = systemPrompt },
                new
                {
                    role    = "user",
                    content = new object[]
                    {
                        new { type = "text", text = userText },
                        new
                        {
                            type      = "image_url",
                            image_url = new { url = $"data:{mimeType};base64,{base64}" }
                        }
                    }
                }
            },
        };

        // Azure OpenAI: POST {endpoint}/openai/deployments/{deployment}/chat/completions?api-version=...
        var url = $"{AzureEndpoint}/openai/deployments/{VisionDeployment}/chat/completions?api-version={ApiVersion}";

        var http = httpFactory.CreateClient("openai");
        var req  = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload,
                options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                })
        };
        req.Headers.Add("api-key", AzureApiKey);

        var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(
            await res.Content.ReadAsStringAsync(ct), _jsonOpts);

        var extracted = completion?.Choices?[0]?.Message?.Content?.Trim();
        logger.LogInformation("Imagem {Id} extraída: {Text}", mediaId, extracted);

        if (string.IsNullOrEmpty(extracted) || extracted == "ERRO")
            throw new InvalidOperationException(
                "Não consegui identificar descrição e valor na imagem.");

        return extracted;
    }

    // ── Inferência de categoria (GPT) ────────────────────────────────────────

    /// <summary>
    /// Usa o GPT para escolher a categoria mais adequada entre as disponíveis do usuário.
    /// Retorna null se não conseguir determinar com confiança.
    /// </summary>
    public async Task<string?> InferCategoryAsync(
        string descricao, IEnumerable<string> categoriasDisponiveis, CancellationToken ct)
    {
        var lista = string.Join(", ", categoriasDisponiveis);
        if (string.IsNullOrWhiteSpace(lista)) return null;

        var payload = new
        {
            max_tokens  = 20,
            temperature = 0,
            messages    = new object[]
            {
                new
                {
                    role    = "system",
                    content = "Você é um assistente de finanças pessoais. " +
                              "Dado o nome de um gasto ou receita, escolha a categoria mais adequada " +
                              "da lista fornecida. Responda APENAS com o nome exato de uma das categorias, " +
                              "sem explicações. Se nenhuma se encaixar bem, responda: Outros"
                },
                new
                {
                    role    = "user",
                    content = $"Gasto/receita: \"{descricao}\"\nCategorias disponíveis: {lista}"
                }
            },
        };

        var url  = $"{AzureEndpoint}/openai/deployments/{VisionDeployment}/chat/completions?api-version={ApiVersion}";
        var http = httpFactory.CreateClient("openai");
        var req  = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload,
                options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                })
        };
        req.Headers.Add("api-key", AzureApiKey);

        var res = await http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return null; // falha silenciosa — fallback para Outros

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(
            await res.Content.ReadAsStringAsync(ct), _jsonOpts);

        var resposta = completion?.Choices?[0]?.Message?.Content?.Trim();
        logger.LogInformation("IA categorizou \"{Desc}\" → \"{Cat}\"", descricao, resposta);

        return string.IsNullOrWhiteSpace(resposta) || resposta == "Outros" ? null : resposta;
    }

    // ── DTOs internos ─────────────────────────────────────────────────────────

    private record MediaUrlResponse(
        [property: JsonPropertyName("url")]       string? Url,
        [property: JsonPropertyName("mime_type")] string? MimeType);

    private record TranscriptionResponse(
        [property: JsonPropertyName("text")] string? Text);

    private record ChatCompletionResponse(
        [property: JsonPropertyName("choices")] List<ChatChoice>? Choices);

    private record ChatChoice(
        [property: JsonPropertyName("message")] ChatMessage? Message);

    private record ChatMessage(
        [property: JsonPropertyName("content")] string? Content);
}
