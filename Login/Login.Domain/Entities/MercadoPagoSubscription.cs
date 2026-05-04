using Login.Domain.Common;

namespace Login.Domain.Entities;

/// <summary>
/// Registra cada assinatura criada no Mercado Pago e seu estado atual.
/// </summary>
public class MercadoPagoSubscription : Entity
{
    public Guid UserId { get; private set; }

    /// <summary>ID do preapproval retornado pela API do Mercado Pago.</summary>
    public string MpSubscriptionId { get; private set; } = string.Empty;

    /// <summary>Plano escolhido pelo usuário (Monthly ou Annual).</summary>
    public PlanType PlanType { get; private set; }

    /// <summary>
    /// Status atual da assinatura conforme o Mercado Pago.
    /// Valores possíveis: pending, authorized, paused, cancelled.
    /// </summary>
    public string Status { get; private set; } = "pending";

    /// <summary>ID do último pagamento processado (preenchido via webhook).</summary>
    public string? LastPaymentId { get; private set; }

    private MercadoPagoSubscription() : base(Guid.Empty) { }

    public MercadoPagoSubscription(
        Guid id,
        Guid userId,
        string mpSubscriptionId,
        PlanType planType) : base(id)
    {
        UserId = userId;
        MpSubscriptionId = mpSubscriptionId;
        PlanType = planType;
        Status = "pending";
    }

    public void Authorize(string? paymentId = null)
    {
        Status = "authorized";
        LastPaymentId = paymentId ?? LastPaymentId;
        SetUpdated();
    }

    public void Cancel()
    {
        Status = "cancelled";
        SetUpdated();
    }

    public void Pause()
    {
        Status = "paused";
        SetUpdated();
    }

    public void SetPaymentId(string paymentId)
    {
        LastPaymentId = paymentId;
        SetUpdated();
    }
}
