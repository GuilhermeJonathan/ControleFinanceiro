# WhatsApp + Azure OpenAI — Quick Reference

> Guia rápido para replicar a integração em outro projeto .NET.
> Versão completa: `WHATSAPP_INTEGRATION.md`

---

## Config Keys (appsettings.json / secrets)

```json
{
  "WhatsApp": {
    "AccessToken":   "EAAxxxxx...",
    "PhoneNumberId": "123456789012345",
    "VerifyToken":   "meu-token-secreto-123"
  },
  "AzureOpenAI": {
    "Endpoint":          "https://SEU-RECURSO.openai.azure.com",
    "ApiKey":            "sua-chave",
    "VisionDeployment":  "gpt-4o-mini",
    "WhisperEndpoint":   "https://SEU-RECURSO-WHISPER.openai.azure.com",
    "WhisperApiKey":     "sua-chave-whisper",
    "WhisperDeployment": "whisper"
  }
}
```

> Whisper = `North Central US` | GPT-4o = `East US` (podem ser recursos Azure separados)

---

## Registro de Serviços

```csharp
services.AddHttpClient("whatsapp");
services.AddHttpClient("openai");
services.AddScoped<IAiService, AzureOpenAiService>();
services.AddScoped<WhatsAppSenderService>();
services.AddScoped<WhatsAppMediaService>();
```

---

## Webhook — Pontos Críticos

```csharp
// GET — verificação Meta (retorna challenge como int)
[HttpGet("webhook")] [AllowAnonymous]
if (mode == "subscribe" && verifyToken == expected)
    return Ok(int.Parse(challenge));

// POST — SEMPRE retornar 200, nunca lançar exceção
[HttpPost("webhook")] [AllowAnonymous]
try { await ProcessMessageAsync(msg, ct); }
catch (ex) { await sender.SendTextAsync(from, $"❌ {ex.Message}", ct); }
return Ok(); // OBRIGATÓRIO — Meta reenvia 72h se != 200
```

---

## Enviar Mensagem

```csharp
// POST https://graph.facebook.com/v25.0/{PhoneNumberId}/messages
// Authorization: Bearer {AccessToken}
// Body (snake_case):
{ "messaging_product": "whatsapp", "to": "5511999990000",
  "type": "text", "text": { "body": "Olá!" } }
```

---

## Download de Mídia (áudio/imagem)

```
1. GET https://graph.facebook.com/v25.0/{mediaId}
   Authorization: Bearer {token}
   → resposta: { "url": "...", "mime_type": "audio/ogg; codecs=opus" }

2. GET {url_retornada}
   Authorization: Bearer {token}
   → bytes do arquivo
```

---

## Whisper (transcrição de áudio)

```
POST {WhisperEndpoint}/openai/deployments/{deployment}/audio/transcriptions?api-version=2024-06-01
api-key: {WhisperApiKey}
Content-Type: multipart/form-data

file = <bytes>  (campo "file", nome "audio.ogg")
language = pt
```

Resposta: `{ "text": "texto transcrito" }`

---

## GPT-4o Vision (análise de imagem)

```
POST {Endpoint}/openai/deployments/{deployment}/chat/completions?api-version=2024-06-01
api-key: {ApiKey}

messages[user].content = [
  { "type": "text", "text": "Extraia o lançamento desta imagem." },
  { "type": "image_url", "image_url": { "url": "data:{mimeType};base64,{base64}" } }
]
```

**System prompt (finanças):**
```
Extraia e responda APENAS: "<descrição> <valor> <data DD/MM/AAAA>"
Exemplo: "Supermercado Atacadão 245,80 28/04/2026"
Se não identificar, responda: ERRO
```

---

## Fluxo Completo

```
Mensagem recebida
  ├─ text  → texto direto
  ├─ audio → download → Whisper → texto
  └─ image → download → GPT-4o Vision → "descrição valor data"
       ↓
   Parse (regex: valor, data, tipo, descrição)
       ↓
   Categoria: keyword local → IA → busca existente → auto-cria → "Outros"
       ↓
   Salva lançamento (com CategoriaId + UserId)
       ↓
   Envia confirmação: "💸 *Almoço* registrado! | R$ 12,50 | hoje | Categoria: Alimentação"
```

---

## Auth sem JWT no Webhook

```csharp
// Após resolver o vínculo phoneNumber → userId:
HttpContext.Items["EffectiveUserId"] = vinculo.UserId;

// ICurrentUser lê de lá:
public Guid UserId => _accessor.HttpContext?.Items["EffectiveUserId"] is Guid id ? id : Guid.Empty;
```

---

## Variação do Dígito 9 (Brasil)

```csharp
// Meta pode enviar 5511999990000 ou 551190000 para o mesmo número
if (normalized.Length == 13) list.Add(normalized[..4] + normalized[5..]); // remove 9
if (normalized.Length == 12) list.Add(normalized[..4] + "9" + normalized[4..]); // adiciona 9
```

---

## Erros Frequentes

| Erro | Causa | Fix |
|------|-------|-----|
| Meta reenvia 72h | POST != 200 | try/catch global, sempre `return Ok()` |
| Whisper 400 | mime_type errado | verificar extensão (ogg/mp3/mp4/webm) |
| Vision 400 | modelo não suporta imagem | usar `gpt-4o-mini` ou `gpt-4o` |
| Número não vinculado | variação dígito 9 | usar `PhoneAlternatives()` |
| Token expirado | Temp token dura ~24h | System User → token permanente em produção |

---

## Checklist Deploy

- [ ] Tokens e keys em secrets (não no appsettings.json commitado)
- [ ] URL webhook HTTPS registrada no Meta
- [ ] Campo `messages` assinado no Meta
- [ ] GET /webhook responde ao verify token
- [ ] POST /webhook sempre retorna 200
- [ ] Tabela `WhatsAppVinculos` migrada no banco
- [ ] System User com token permanente (não Temporary Access Token)
