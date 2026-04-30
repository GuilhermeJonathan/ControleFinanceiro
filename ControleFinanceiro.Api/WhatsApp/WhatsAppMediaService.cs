using ControleFinanceiro.Application.Common.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ControleFinanceiro.Api.WhatsApp;

/// <summary>
/// Baixa mídias da Meta e usa Azure OpenAI (Whisper + GPT-4o) para
/// converter áudio/imagem em texto processável pelo WhatsAppMessageParser.
/// Usa <see cref="IAiService"/> para inferências de texto (categorias, etc.).
/// </summary>
public class WhatsAppMediaService(
    IHttpClientFactory httpFactory,
    IConfiguration config,
    IAiService ai,
    ILogger<WhatsAppMediaService> logger)
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private string AccessToken => config["WhatsApp:AccessToken"]
        ?? throw new InvalidOperationException("WhatsApp:AccessToken não configurado.");

    // ── Azure OpenAI — Vision/GPT (agente-financeiro, East US) ───────────────
    private string VisionEndpoint   => (config["AzureOpenAI:Endpoint"] ?? "").TrimEnd('/');
    private string VisionApiKey     => config["AzureOpenAI:ApiKey"]
        ?? throw new InvalidOperationException("AzureOpenAI:ApiKey não configurado.");
    private string VisionDeployment => config["AzureOpenAI:VisionDeployment"] ?? "gpt-5.4-mini";

    // ── Azure OpenAI — Whisper (recurso separado, North Central US) ──────────
    private string WhisperEndpoint   => (config["AzureOpenAI:WhisperEndpoint"] ?? "").TrimEnd('/');
    private string WhisperApiKey     => config["AzureOpenAI:WhisperApiKey"]
        ?? throw new InvalidOperationException("AzureOpenAI:WhisperApiKey não configurado.");
    private string WhisperDeployment => config["AzureOpenAI:WhisperDeployment"] ?? "whisper";

    private const string ApiVersion = "2024-06-01";

    // ── Download de mídia da Meta ─────────────────────────────────────────────

    public async Task<(byte[] Data, string MimeType)> DownloadMediaAsync(
        string mediaId, CancellationToken ct)
    {
        var http = httpFactory.CreateClient("whatsapp");

        // 1. Busca a URL temporária da mídia
        var infoReq = new HttpRequestMessage(
            HttpMethod.Get, $"https://graph.facebook.com/v25.0/{mediaId}");
        infoReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var infoRes  = await http.SendAsync(infoReq, ct);
        var infoBody = await infoRes.Content.ReadAsStringAsync(ct);
        if (!infoRes.IsSuccessStatusCode)
        {
            logger.LogError("Meta mídia info erro {Status} | id={Id} | Body={Body}",
                (int)infoRes.StatusCode, mediaId, infoBody);
            infoRes.EnsureSuccessStatusCode();
        }

        var info = JsonSerializer.Deserialize<MediaUrlResponse>(infoBody, _jsonOpts)
            ?? throw new InvalidOperationException("Resposta inválida ao buscar URL da mídia.");

        if (string.IsNullOrEmpty(info.Url))
            throw new InvalidOperationException("Meta não retornou URL para a mídia.");

        // 2. Baixa o arquivo usando o Bearer token
        var dlReq = new HttpRequestMessage(HttpMethod.Get, info.Url);
        dlReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var dlRes = await http.SendAsync(dlReq, ct);
        if (!dlRes.IsSuccessStatusCode)
        {
            var dlErr = await dlRes.Content.ReadAsStringAsync(ct);
            logger.LogError("Meta download mídia erro {Status} | id={Id} | Body={Body}",
                (int)dlRes.StatusCode, mediaId, dlErr);
            dlRes.EnsureSuccessStatusCode();
        }

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
        return await TranscribeRawAsync(data, mimeType, ct);
    }

    /// <summary>Transcreve bytes de áudio diretamente (sem download da Meta). Útil para testes.</summary>
    public async Task<string> TranscribeRawAsync(byte[] data, string mimeType, CancellationToken ct)
    {

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
        var url = $"{WhisperEndpoint}/openai/deployments/{WhisperDeployment}/audio/transcriptions?api-version={ApiVersion}";

        var http = httpFactory.CreateClient("openai");
        var req  = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
        req.Headers.Add("api-key", WhisperApiKey);

        var res  = await http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("Whisper erro {Status} | URL={Url} | Body={Body}",
                (int)res.StatusCode, url, body);
            throw new InvalidOperationException(
                $"Whisper retornou {(int)res.StatusCode}: {body}");
        }

        var result = JsonSerializer.Deserialize<TranscriptionResponse>(body, _jsonOpts);
        var text   = result?.Text?.Trim();

        if (string.IsNullOrEmpty(text))
            throw new InvalidOperationException("Whisper não retornou transcrição.");

        logger.LogInformation("Áudio transcrito: {Text}", text);
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
        return await ExtractRawAsync(data, mimeType, caption, ct);
    }

    /// <summary>Extrai lançamento de bytes de imagem diretamente (sem download da Meta). Útil para testes.</summary>
    public async Task<string> ExtractRawAsync(
        byte[] data, string mimeType, string? caption, CancellationToken ct)
    {
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
            max_completion_tokens = 80,
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
        var url = $"{VisionEndpoint}/openai/deployments/{VisionDeployment}/chat/completions?api-version={ApiVersion}";

        var http = httpFactory.CreateClient("openai");
        var req  = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload,
                options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                })
        };
        req.Headers.Add("api-key", VisionApiKey);

        var res      = await http.SendAsync(req, ct);
        var resBody  = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("Vision erro {Status} | URL={Url} | Body={Body}",
                (int)res.StatusCode, url, resBody);
            throw new InvalidOperationException(
                $"Vision retornou {(int)res.StatusCode}: {resBody}");
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(resBody, _jsonOpts);
        var extracted  = completion?.Choices?[0]?.Message?.Content?.Trim();
        logger.LogInformation("Imagem extraída: {Text}", extracted);

        if (string.IsNullOrEmpty(extracted) || extracted == "ERRO")
            throw new InvalidOperationException(
                "Não consegui identificar descrição e valor na imagem.");

        return extracted;
    }

    // ── Sugestão inteligente de categoria (IAiService) ───────────────────────
    /// <summary>
    /// Usa IA para sugerir o nome da categoria mais adequada ao lançamento,
    /// sem estar limitada às categorias já existentes do usuário.
    /// Se a categoria sugerida não existir, o controller a cria automaticamente.
    /// Retorna null quando não há categoria clara (fallback para "Outros").
    /// </summary>
    public async Task<string?> SuggestCategoryAsync(string descricao, CancellationToken ct)
    {
        const string system =
            "Você é um assistente de finanças pessoais brasileiro. " +
            "Dado o nome de um gasto ou receita, sugira o nome da categoria financeira mais adequada. " +
            "Exemplos de categorias válidas: Alimentação, Transporte, Saúde, Moradia, Lazer, " +
            "Educação, Vestuário, Pets, Salário, Investimentos, Serviços, Beleza, Viagem, Tecnologia, Assinaturas. " +
            "Responda APENAS com o nome da categoria, em português, com inicial maiúscula, sem explicações. " +
            "Se não houver categoria clara, responda: Outros";

        var user = $"Lançamento: \"{descricao}\"";

        try
        {
            var resposta = (await ai.ChatAsync(system, user, maxTokens: 15, temperature: 0, ct)).Trim();
            logger.LogInformation("IA sugeriu categoria \"{Cat}\" para \"{Desc}\"", resposta, descricao);
            return string.IsNullOrWhiteSpace(resposta) || resposta == "Outros" ? null : resposta;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SuggestCategoryAsync falhou para \"{Desc}\"", descricao);
            return null;
        }
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
