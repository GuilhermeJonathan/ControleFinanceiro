using MediatR;

namespace Login.Application.Payments.Queries.GetPaymentTransactions;

public record GetPaymentTransactionsQuery(int Page, int PageSize)
    : IRequest<PaymentTransactionsResult>;

public record PaymentTransactionsResult(
    IReadOnlyList<PaymentTransactionDto> Items,
    int TotalCount);
