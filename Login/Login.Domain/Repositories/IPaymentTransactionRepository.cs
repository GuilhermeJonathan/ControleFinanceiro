using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface IPaymentTransactionRepository
{
    Task AddAsync(PaymentTransaction transaction, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentTransaction>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
