using MediatR;

namespace Login.Application.Payments.Commands.CreateCheckout;

/// <summary>
/// Cria uma assinatura no Mercado Pago e retorna a URL de checkout.
/// </summary>
/// <param name="PlanId">"mensal" ou "anual"</param>
public record CreateCheckoutCommand(string PlanId, string? PayerEmail = null) : IRequest<CreateCheckoutResult>;

public record CreateCheckoutResult(string CheckoutUrl);
