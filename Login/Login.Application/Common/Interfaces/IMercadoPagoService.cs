namespace Login.Application.Common.Interfaces;

public record MpCheckoutResult(string SubscriptionId, string CheckoutUrl);

public record MpSubscriptionDetail(
    string Id,
    string Status,
    string ExternalReference,
    string? LastPaymentId,
    decimal TransactionAmount);

public interface IMercadoPagoService
{
    /// <summary>
    /// Cria uma assinatura (preapproval) no Mercado Pago e retorna o link de checkout.
    /// </summary>
    Task<MpCheckoutResult> CreateSubscriptionAsync(
        string userEmail,
        Guid userId,
        string planId,          // "mensal" ou "anual"
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca os detalhes de uma assinatura pelo ID do MP.
    /// </summary>
    Task<MpSubscriptionDetail?> GetSubscriptionAsync(
        string mpSubscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida a assinatura do webhook usando HMAC-SHA256.
    /// </summary>
    bool ValidateWebhookSignature(
        string xSignature,
        string xRequestId,
        string dataId);
}
