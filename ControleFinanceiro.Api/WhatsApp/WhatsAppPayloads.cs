using System.Text.Json.Serialization;

namespace ControleFinanceiro.Api.WhatsApp;

// ── DTOs do webhook da Meta ────────────────────────────────────────────────────

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
