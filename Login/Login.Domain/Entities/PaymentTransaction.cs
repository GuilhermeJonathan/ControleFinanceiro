using Login.Domain.Common;

namespace Login.Domain.Entities;

public class PaymentTransaction : Entity
{
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string UserEmail { get; private set; } = string.Empty;
    public PlanType PlanType { get; private set; }
    public decimal Amount { get; private set; }

    /// <summary>Status do pagamento: "authorized", "cancelled", etc.</summary>
    public string Status { get; private set; } = string.Empty;

    public string? MpPaymentId { get; private set; }
    public string? MpSubscriptionId { get; private set; }
    public DateTime PaidAt { get; private set; }

    private PaymentTransaction() : base(Guid.Empty) { }

    public PaymentTransaction(
        Guid id,
        Guid userId,
        string userName,
        string userEmail,
        PlanType planType,
        decimal amount,
        string status,
        string? mpPaymentId,
        string? mpSubscriptionId,
        DateTime paidAt) : base(id)
    {
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        PlanType = planType;
        Amount = amount;
        Status = status;
        MpPaymentId = mpPaymentId;
        MpSubscriptionId = mpSubscriptionId;
        PaidAt = paidAt;
    }
}
