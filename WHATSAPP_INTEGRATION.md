# WhatsApp Business API — Integração Completa com IA

Guia completo para replicar a integração WhatsApp + Azure OpenAI em qualquer projeto .NET.
Cobre recebimento de texto, áudio (transcrição Whisper) e imagem (GPT-4o Vision).

---

## 1. Pré-requisitos no Meta

### 1.1 Criar app no Meta for Developers
1. Acesse https://developers.facebook.com
2. Crie um app do tipo **Business**
3. Adicione o produto **WhatsApp**
4. Em **WhatsApp > API Setup**, anote:
   - `Phone Number ID` (ex: `123456789012345`)
   - `WhatsApp Business Account ID`
   - Gere um **Temporary Access Token** (para dev) ou configure um token permanente via System User

### 1.2 Configurar webhook
No painel Meta → **WhatsApp > Configuration > Webhook**:
- **Callback URL**: `https://seu-dominio.com/api/whatsapp/webhook`
- **Verify Token**: qualquer string que você escolher (ex: `meu-token-secreto-123`)
- **Campos assinados**: marque `messages`

### 1.3 Permissões necessárias
- `whatsapp_business_messaging` — enviar mensagens
- `whatsapp_business_management` — gerenciar números

---

## 2. Configuração da Aplicação (.NET)

### 2.1 appsettings.json / secrets
```json
{
  "WhatsApp": {
    "AccessToken":   "EAAxxxxx...",
    "PhoneNumberId": "123456789012345",
    "VerifyToken":   "meu-token-secreto-123"
  },
  "AzureOpenAI": {
    "Endpoint":          "https://SEU-RECURSO.openai.azure.com",
    "ApiKey":            "sua-chave-aqui",
    "VisionDeployment":  "gpt-4o-mini",
    "WhisperEndpoint":   "https://SEU-RECURSO-WHISPER.openai.azure.com",
    "WhisperApiKey":     "sua-chave-whisper",
    "WhisperDeployment": "whisper"
  }
}
```

> **Nota**: Whisper e GPT-4o podem estar em recursos Azure separados (regiões diferentes).
> O Whisper está disponível em `North Central US`; GPT-4o em `East US`, `Sweden Central`, etc.

### 2.2 Registro de serviços (Program.cs / Extensions)
```csharp
services.AddHttpClient("whatsapp");
services.AddHttpClient("openai");
services.AddScoped<IAiService, AzureOpenAiService>();
services.AddScoped<WhatsAppSenderService>();
services.AddScoped<WhatsAppMediaService>();
```

---

## 3. DTOs do Webhook (WhatsAppPayloads.cs)

A Meta envia um JSON neste formato para o webhook:

```csharp
public record WhatsAppWebhookPayload(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("entry")]  List<WhatsAppEntry> Entry);

public record WhatsAppEntry(
    [property: JsonPropertyName("id")]      string Id,
    [property: JsonPropertyName("changes")] List<WhatsAppChange> Changes);

public record WhatsAppChange(
    [property: JsonPropertyName("value")] WhatsAppValue Value,
    [property: JsonPropertyName("field")] string Field);

public record WhatsAppValue(
    [property: JsonPropertyName("messaging_product")] string MessagingProduct,
    [property: JsonPropertyName("messages")]          List<WhatsAppMessage>? Messages,
    [property: JsonPropertyName("statuses")]          List<WhatsAppStatus>? Statuses);

public record WhatsAppMessage(
    [property: JsonPropertyName("from")]      string From,
    [property: JsonPropertyName("id")]        string Id,
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("type")]      string Type,
    [property: JsonPropertyName("text")]      WhatsAppText?  Text,
    [property: JsonPropertyName("audio")]     WhatsAppMedia? Audio,
    [property: JsonPropertyName("image")]     WhatsAppMedia? Image);

public record WhatsAppText(
    [property: JsonPropertyName("body")] string Body);

public record WhatsAppMedia(
    [property: JsonPropertyName("id")]        string  Id,
    [property: JsonPropertyName("mime_type")] string? MimeType,
    [property: JsonPropertyName("caption")]   string? Caption,
    [property: JsonPropertyName("sha256")]    string? Sha256);

public record WhatsAppStatus(
    [property: JsonPropertyName("id")]           string Id,
    [property: JsonPropertyName("status")]       string Status,
    [property: JsonPropertyName("timestamp")]    string Timestamp,
    [property: JsonPropertyName("recipient_id")] string RecipientId,
    [property: JsonPropertyName("errors")]       List<WhatsAppStatusError>? Errors);

public record WhatsAppStatusError(
    [property: JsonPropertyName("code")]    int    Code,
    [property: JsonPropertyName("title")]   string Title,
    [property: JsonPropertyName("message")] string? Message);
```

---

## 4. Controller do Webhook (WhatsAppController.cs)

### 4.1 Verificação do webhook (GET)
A Meta chama esse endpoint ao configurar o webhook para confirmar que é seu servidor:

```csharp
[HttpGet("webhook")]
[AllowAnonymous]
public IActionResult Verify(
    [FromQuery(Name = "hub.mode")]         string? mode,
    [FromQuery(Name = "hub.challenge")]    string? challenge,
    [FromQuery(Name = "hub.verify_token")] string? verifyToken)
{
    var expected = config["WhatsApp:VerifyToken"];

    if (mode == "subscribe" && verifyToken == expected && challenge is not null)
        return Ok(int.Parse(challenge));  // retorna o challenge como número inteiro

    return Forbid();
}
```

### 4.2 Recebimento de mensagens (POST)

```csharp
[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> Receive(
    [FromBody] WhatsAppWebhookPayload payload,
    CancellationToken ct)
{
    var messages = payload.Entry
        .SelectMany(e => e.Changes)
        .Where(c => c.Field == "messages")
        .SelectMany(c => c.Value.Messages ?? [])
        .Where(m => m.Type is "text" or "audio" or "image")
        .ToList();

    foreach (var msg in messages)
        await ProcessMessageAsync(msg, ct);

    // A Meta exige HTTP 200 independente de erros internos
    return Ok();
}
```

> **Crítico**: Sempre retorne `200 OK`. Se retornar erro, a Meta reenvia o webhook
> indefinidamente por 72 horas. Trate todos os erros internamente.

### 4.3 Processamento por tipo de mensagem

```csharp
private async Task ProcessMessageAsync(WhatsAppMessage msg, CancellationToken ct)
{
    var from = msg.From; // número do remetente: "5511999990000"

    try
    {
        string text;
        switch (msg.Type)
        {
            case "text":
                text = msg.Text!.Body;
                break;

            case "audio":
                await sender.SendTextAsync(from, "🎙️ Transcrevendo seu áudio...", ct);
                text = await mediaService.TranscribeAudioAsync(msg.Audio!.Id, ct);
                break;

            case "image":
                await sender.SendTextAsync(from, "🖼️ Analisando a imagem...", ct);
                // msg.Image.Caption é a legenda que o usuário digitou (opcional)
                text = await mediaService.ExtractFromImageAsync(
                    msg.Image!.Id, msg.Image.Caption, ct);
                break;

            default:
                return; // tipo não suportado — ignora silenciosamente
        }

        // ... processar o texto extraído
    }
    catch (Exception ex)
    {
        await sender.SendTextAsync(from, $"❌ Erro: {ex.Message}", ct);
    }
}
```

---

## 5. Envio de Mensagens (WhatsAppSenderService.cs)

```csharp
public class WhatsAppSenderService(IHttpClientFactory httpFactory, IConfiguration config)
{
    private string PhoneNumberId => config["WhatsApp:PhoneNumberId"]!;
    private string AccessToken   => config["WhatsApp:AccessToken"]!;

    public async Task SendTextAsync(string to, string message, CancellationToken ct = default)
    {
        var payload = new
        {
            messaging_product = "whatsapp",
            to,
            type = "text",
            text = new { body = message }
        };

        var http = httpFactory.CreateClient("whatsapp");
        var url  = $"https://graph.facebook.com/v25.0/{PhoneNumberId}/messages";

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
```

> **Observação**: Use `PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower` para serializar
> o payload. A API da Meta espera `messaging_product`, `to`, etc. em snake_case.

---

## 6. Download de Mídia da Meta (WhatsAppMediaService.cs)

Áudio e imagem chegam como um `mediaId`. Para obter o arquivo:

**Passo 1** — buscar a URL temporária:
```
GET https://graph.facebook.com/v25.0/{mediaId}
Authorization: Bearer {AccessToken}
```
Resposta:
```json
{ "url": "https://lookaside.fbsbx.com/...", "mime_type": "audio/ogg; codecs=opus" }
```

**Passo 2** — baixar o arquivo com o mesmo Bearer token:
```
GET {url_retornada}
Authorization: Bearer {AccessToken}
```

```csharp
public async Task<(byte[] Data, string MimeType)> DownloadMediaAsync(string mediaId, CancellationToken ct)
{
    var http = httpFactory.CreateClient("whatsapp");

    // 1. URL temporária
    var infoReq = new HttpRequestMessage(HttpMethod.Get,
        $"https://graph.facebook.com/v25.0/{mediaId}");
    infoReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    var infoRes  = await http.SendAsync(infoReq, ct);
    infoRes.EnsureSuccessStatusCode();
    var info = JsonSerializer.Deserialize<MediaUrlResponse>(
        await infoRes.Content.ReadAsStringAsync(ct),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    // 2. Download do arquivo
    var dlReq = new HttpRequestMessage(HttpMethod.Get, info!.Url);
    dlReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    var dlRes = await http.SendAsync(dlReq, ct);
    dlRes.EnsureSuccessStatusCode();

    return (await dlRes.Content.ReadAsByteArrayAsync(ct), info.MimeType ?? "application/octet-stream");
}

private record MediaUrlResponse(
    [property: JsonPropertyName("url")]       string? Url,
    [property: JsonPropertyName("mime_type")] string? MimeType);
```

---

## 7. Transcrição de Áudio — Azure OpenAI Whisper

O WhatsApp envia áudio como `audio/ogg` (codec opus). O Whisper aceita esse formato.

```csharp
public async Task<string> TranscribeAudioAsync(string mediaId, CancellationToken ct)
{
    var (data, mimeType) = await DownloadMediaAsync(mediaId, ct);
    return await TranscribeRawAsync(data, mimeType, ct);
}

public async Task<string> TranscribeRawAsync(byte[] data, string mimeType, CancellationToken ct)
{
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
    audioContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
    form.Add(audioContent, "file", $"audio.{ext}");
    form.Add(new StringContent("pt"), "language"); // força português

    // Azure OpenAI Whisper
    var url = $"{WhisperEndpoint}/openai/deployments/{WhisperDeployment}/audio/transcriptions?api-version=2024-06-01";

    var http = httpFactory.CreateClient("openai");
    var req  = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
    req.Headers.Add("api-key", WhisperApiKey);

    var res  = await http.SendAsync(req, ct);
    var body = await res.Content.ReadAsStringAsync(ct);
    res.EnsureSuccessStatusCode();

    var result = JsonSerializer.Deserialize<TranscriptionResponse>(body,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return result?.Text?.Trim()
        ?? throw new InvalidOperationException("Whisper não retornou transcrição.");
}

private record TranscriptionResponse([property: JsonPropertyName("text")] string? Text);
```

**Endpoint Azure Whisper:**
```
POST {WhisperEndpoint}/openai/deployments/{deployment}/audio/transcriptions?api-version=2024-06-01
api-key: {WhisperApiKey}
Content-Type: multipart/form-data

file=<bytes>  (campo "file", nome "audio.ogg")
language=pt
```

---

## 8. Análise de Imagem — Azure OpenAI GPT-4o Vision

Envia a imagem como base64 junto com um prompt. Retorna texto extraído do comprovante.

```csharp
public async Task<string> ExtractFromImageAsync(string mediaId, string? caption, CancellationToken ct)
{
    var (data, mimeType) = await DownloadMediaAsync(mediaId, ct);
    return await ExtractRawAsync(data, mimeType, caption, ct);
}

public async Task<string> ExtractRawAsync(byte[] data, string mimeType, string? caption, CancellationToken ct)
{
    var base64 = Convert.ToBase64String(data);

    const string systemPrompt = """
        Você é um assistente financeiro pessoal. O usuário enviou uma foto de
        cupom fiscal, comprovante, nota, recibo ou tela de pagamento.
        Extraia as informações e responda APENAS com uma linha de texto simples:
        "<descrição breve do estabelecimento ou item> <valor total em reais> <data>"
        Regras:
        - Valor: use vírgula como separador decimal, ex: 245,80
        - Data: formato DD/MM/AAAA se o ano estiver visível, ou DD/MM se não. Se não houver data, omita.
        Exemplos: "Supermercado Atacadão 245,80 28/04/2026" | "iFood delivery 38,90"
        Se não conseguir identificar claramente, responda: ERRO
        NÃO inclua explicações, prefixos nem pontuação extra.
        """;

    var userText = string.IsNullOrWhiteSpace(caption)
        ? "Extraia o lançamento desta imagem."
        : $"Legenda do usuário: \"{caption}\". Extraia o lançamento desta imagem.";

    var payload = new
    {
        model = "gpt-4o-mini",
        max_completion_tokens = 80,
        messages = new object[]
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
        }
    };

    var url = $"{VisionEndpoint}/openai/deployments/{VisionDeployment}/chat/completions?api-version=2024-06-01";

    var http = httpFactory.CreateClient("openai");
    var req  = new HttpRequestMessage(HttpMethod.Post, url)
    {
        Content = JsonContent.Create(payload, options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        })
    };
    req.Headers.Add("api-key", VisionApiKey);

    var res     = await http.SendAsync(req, ct);
    var resBody = await res.Content.ReadAsStringAsync(ct);
    res.EnsureSuccessStatusCode();

    var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(resBody,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    var extracted = completion?.Choices?[0]?.Message?.Content?.Trim();

    if (string.IsNullOrEmpty(extracted) || extracted == "ERRO")
        throw new InvalidOperationException("Não consegui identificar descrição e valor na imagem.");

    return extracted;
}
```

---

## 9. Sugestão de Categoria por IA (IAiService)

Interface reutilizável para qualquer chamada ao modelo de texto:

```csharp
// Interface (Application layer)
public interface IAiService
{
    Task<string> ChatAsync(
        string systemPrompt,
        string userMessage,
        int maxTokens = 800,
        float temperature = 0.3f,
        CancellationToken cancellationToken = default);
}

// Implementação (Api layer) — Azure OpenAI Chat Completions
public class AzureOpenAiService(IHttpClientFactory httpFactory, IConfiguration config) : IAiService
{
    public async Task<string> ChatAsync(string systemPrompt, string userMessage,
        int maxTokens = 800, float temperature = 0.3f, CancellationToken cancellationToken = default)
    {
        var endpoint   = config["AzureOpenAI:Endpoint"]!.TrimEnd('/');
        var apiKey     = config["AzureOpenAI:ApiKey"]!;
        var deployment = config["AzureOpenAI:VisionDeployment"] ?? "gpt-4o-mini";

        var payload = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userMessage  }
            },
            max_completion_tokens = maxTokens,
            temperature
        };

        var url = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version=2024-06-01";

        var http = httpFactory.CreateClient("openai");
        var req  = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            })
        };
        req.Headers.Add("api-key", apiKey);

        var res  = await http.SendAsync(req, cancellationToken);
        var body = await res.Content.ReadAsStringAsync(cancellationToken);
        res.EnsureSuccessStatusCode();

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return completion?.Choices?[0]?.Message?.Content?.Trim() ?? "";
    }
}
```

**Prompt para sugestão de categoria:**
```csharp
const string system =
    "Você é um assistente de finanças pessoais brasileiro. " +
    "Dado o nome de um gasto ou receita, sugira o nome da categoria financeira mais adequada. " +
    "Exemplos: Alimentação, Transporte, Saúde, Moradia, Lazer, Educação, Vestuário, " +
    "Pets, Salário, Investimentos, Serviços, Beleza, Viagem, Tecnologia, Assinaturas. " +
    "Responda APENAS com o nome da categoria, em português, com inicial maiúscula. " +
    "Se não houver categoria clara, responda: Outros";

var resposta = await ai.ChatAsync(system, $"Lançamento: \"{descricao}\"",
    maxTokens: 15, temperature: 0, ct);
```

---

## 10. Fluxo Completo de Processamento

```
Usuário envia mensagem WhatsApp
         │
         ▼
POST /api/whatsapp/webhook
         │
         ├─ type = "text"  ──────────────────── texto direto
         │
         ├─ type = "audio" ─── DownloadMedia ──► TranscribeAudio (Whisper) ──► texto
         │
         └─ type = "image" ─── DownloadMedia ──► ExtractFromImage (GPT-4o)  ──► texto
                                                  (extrai: descrição + valor + data)
         │
         ▼
   Parsear texto
   (regex: valor, data, tipo crédito/débito, descrição limpa)
         │
         ▼
   Resolver categoria
   1. Palavras-chave locais (CategoryMatcher)
   2. IA (SuggestCategoryAsync) se não encontrou
   3. Busca nas categorias existentes do usuário
   4. Cria automaticamente se não existir
   5. Fallback: "Outros"
         │
         ▼
   Salvar lançamento (CategoriaId, UsuarioId, Data, Valor, Tipo, ...)
         │
         ▼
   Enviar confirmação via WhatsApp:
   "💸 *Almoço* registrado!
    Valor: R$ 12,50
    Data: hoje
    Categoria: Alimentação"
```

---

## 11. Inferência de Categoria por Palavras-chave (CategoryMatcher)

Para evitar custo de IA nas descrições comuns, use um dicionário local primeiro:

```csharp
public static class CategoryMatcher
{
    private static readonly Dictionary<string, string> _keywords = new()
    {
        { "almoco",         "Alimentação" }, { "jantar",    "Alimentação" },
        { "restaurante",    "Alimentação" }, { "mercado",   "Alimentação" },
        { "supermercado",   "Alimentação" }, { "ifood",     "Alimentação" },
        { "gasolina",       "Transporte"  }, { "uber",      "Transporte"  },
        { "combustivel",    "Transporte"  }, { "onibus",    "Transporte"  },
        { "farmacia",       "Saúde"       }, { "medico",    "Saúde"       },
        { "hospital",       "Saúde"       }, { "academia",  "Saúde"       },
        { "aluguel",        "Moradia"     }, { "energia",   "Moradia"     },
        { "internet",       "Moradia"     }, { "agua",      "Moradia"     },
        { "netflix",        "Lazer"       }, { "cinema",    "Lazer"       },
        { "salario",        "Salário"     }, { "freelance", "Salário"     },
        // ... adicione conforme o domínio do seu projeto
    };

    public static string? Infer(string descricao)
    {
        var norm = Normalize(descricao); // minúsculo + sem acentos
        foreach (var (kw, cat) in _keywords.OrderByDescending(k => k.Key.Length))
            if (norm.Contains(Normalize(kw)))
                return cat;
        return null;
    }

    private static string Normalize(string text)
    {
        var s = text.ToLowerInvariant();
        return string.Concat(
            s.Normalize(NormalizationForm.FormD)
             .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
    }
}
```

---

## 12. Vinculação de Número ao Usuário

Para saber a qual usuário do sistema pertence cada mensagem, armazene um vínculo `phoneNumber ↔ userId`:

```csharp
// Entidade
public class WhatsAppVinculo
{
    public Guid   Id          { get; private set; }
    public Guid   UserId      { get; private set; }
    public string PhoneNumber { get; private set; } = ""; // somente dígitos

    public WhatsAppVinculo(Guid userId, string phoneNumber)
    {
        Id          = Guid.NewGuid();
        UserId      = userId;
        PhoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());
        CreatedAt   = DateTime.UtcNow;
    }
}

// No webhook, antes de processar:
var vinculo = await vinculoRepo.GetByPhoneAsync(msg.From, ct);
if (vinculo is null)
{
    await sender.SendTextAsync(msg.From, "⚠️ Número não vinculado. Configure no app.", ct);
    return;
}
// vinculo.UserId → ID do usuário no seu sistema
```

**Dica — variação do dígito 9 no Brasil:**
Números brasileiros podem chegar com ou sem o "9" após o DDD (`5511999990000` vs `551190000`).
Normalize na busca:

```csharp
private static string[] PhoneAlternatives(string normalized)
{
    var list = new List<string> { normalized };
    if (normalized.StartsWith("55") && normalized.Length == 13)
        list.Add(normalized[..4] + normalized[5..]);  // remove o 9
    else if (normalized.StartsWith("55") && normalized.Length == 12)
        list.Add(normalized[..4] + "9" + normalized[4..]);  // adiciona o 9
    return [.. list];
}
```

---

## 13. Autenticação sem JWT no Webhook

O webhook é `[AllowAnonymous]`. Para que handlers de domínio funcionem como se o usuário estivesse autenticado, injete o `userId` no `HttpContext.Items` antes de chamar os handlers:

```csharp
// No controller, após resolver o vínculo:
HttpContext.Items["EffectiveUserId"] = vinculo.UserId;
HttpContext.Items["RealUserId"]      = vinculo.UserId;

// ICurrentUser lê de lá (mesmo padrão que o middleware de família):
public Guid UserId => _accessor.HttpContext?.Items["EffectiveUserId"] is Guid id ? id : Guid.Empty;
```

---

## 14. Logs de Diagnóstico

Adicione `Console.WriteLine` (ou `ILogger`) em pontos-chave para rastrear o fluxo:

```csharp
Console.WriteLine($"[WhatsApp] from={from} | type={msg.Type}");
Console.WriteLine($"[WhatsApp] categorias carregadas: {categorias.Count} | descricao=\"{parsed.Descricao}\"");
Console.WriteLine($"[WhatsApp] keyword match: {nomeCategoria ?? "(nenhum)"}");
Console.WriteLine($"[WhatsApp] categoriaId final: {categoriaId?.ToString() ?? "NULL ⚠️"}");
```

---

## 15. Checklist de Deploy

- [ ] `WhatsApp:AccessToken` configurado (token permanente via System User em produção)
- [ ] `WhatsApp:PhoneNumberId` configurado
- [ ] `WhatsApp:VerifyToken` configurado (mesma string no Meta e no app)
- [ ] `AzureOpenAI:Endpoint` + `AzureOpenAI:ApiKey` + `AzureOpenAI:VisionDeployment`
- [ ] `AzureOpenAI:WhisperEndpoint` + `AzureOpenAI:WhisperApiKey` + `AzureOpenAI:WhisperDeployment`
- [ ] URL do webhook registrada no Meta com HTTPS
- [ ] Campos `messages` assinados no webhook Meta
- [ ] Endpoint `GET /webhook` respondendo corretamente ao verify token
- [ ] Endpoint `POST /webhook` retornando sempre `200 OK`
- [ ] Migração do banco com tabela de vínculos `WhatsAppVinculos`

---

## 16. Erros Comuns

| Erro | Causa | Solução |
|------|-------|---------|
| Meta reenvia webhook 72h | POST retornou != 200 | Envolver todo o processamento em try/catch, retornar Ok() sempre |
| `Whisper retornou 400` | Arquivo de áudio corrompido ou formato errado | Verificar mime_type e extensão do arquivo |
| `Vision retornou 400` | Base64 muito grande ou modelo não suporta Vision | Usar gpt-4o ou gpt-4o-mini (não gpt-3.5) |
| Categoria não associada | Lancamento criado com código antigo sem auto-create | Novo código cria categoria automaticamente; lancamentos antigos precisam ser atualizados manualmente |
| Número não vinculado | Formato do número Meta vs formato armazenado | Usar `PhoneAlternatives` para tratar variação do dígito 9 |
| Token expirado | Temporary Access Token dura ~24h | Usar System User com token permanente em produção |
