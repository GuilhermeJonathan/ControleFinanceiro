using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Payments.Queries.GetPaymentTransactions;

public class GetPaymentTransactionsQueryHandler
    : IRequestHandler<GetPaymentTransactionsQuery, PaymentTransactionsResult>
{
    private readonly IPaymentTransactionRepository _repository;

    public GetPaymentTransactionsQueryHandler(IPaymentTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentTransactionsResult> Handle(
        GetPaymentTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var page     = request.Page     < 1 ? 1  : request.Page;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

        var items = await _repository.GetAllAsync(page, pageSize, cancellationToken);
        var total = await _repository.CountAsync(cancellationToken);

        var dtos = items
            .Select(t => new PaymentTransactionDto(
                Id:           t.Id,
                UserId:       t.UserId,
                UserName:     t.UserName,
                UserEmail:    t.UserEmail,
                PlanType:     t.PlanType.ToString(),
                Amount:       t.Amount,
                Status:       t.Status,
                MpPaymentId:  t.MpPaymentId,
                PaidAt:       t.PaidAt))
            .ToList();

        return new PaymentTransactionsResult(dtos, total);
    }
}
