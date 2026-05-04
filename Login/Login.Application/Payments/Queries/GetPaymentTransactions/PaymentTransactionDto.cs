namespace Login.Application.Payments.Queries.GetPaymentTransactions;

public record PaymentTransactionDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string UserEmail,
    string PlanType,
    decimal Amount,
    string Status,
    string? MpPaymentId,
    DateTime PaidAt);
