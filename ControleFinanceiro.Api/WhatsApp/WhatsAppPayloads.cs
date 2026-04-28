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
    [property: JsonPropertyName("statuses")]          List<object>? Statuses);

public record WhatsAppMessage(
    [property: JsonPropertyName("from")]      string From,
    [property: JsonPropertyName("id")]        string Id,
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("type")]      string Type,
    [property: JsonPropertyName("text")]      WhatsAppText? Text);

public record WhatsAppText(
    [property: JsonPropertyName("body")] string Body);
