using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class PaymentTransactionRepository : IPaymentTransactionRepository
{
    private readonly AppDbContext _context;

    public PaymentTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PaymentTransaction transaction, CancellationToken ct = default)
        => await _context.PaymentTransactions.AddAsync(transaction, ct);

    public async Task<IReadOnlyList<PaymentTransaction>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken ct = default)
        => await _context.PaymentTransactions
            .OrderByDescending(t => t.PaidAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await _context.PaymentTransactions.CountAsync(ct);
}
