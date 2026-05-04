using MediatR;

namespace Login.Application.Payments.Commands.ProcessWebhook;

/// <summary>
/// Processa uma notificação de webhook do Mercado Pago.
/// </summary>
public record ProcessWebhookCommand(
    string Type,        // "subscription_preapproval" | "payment"
    string DataId,      // ID do recurso no MP
    string? PaymentId   // Preenchido quando type = "payment"
) : IRequest;
