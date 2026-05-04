using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MercadoPagoSubscription?> GetByMpIdAsync(
        string mpSubscriptionId,
        CancellationToken cancellationToken = default)
        => await _context.MercadoPagoSubscriptions
            .FirstOrDefaultAsync(s => s.MpSubscriptionId == mpSubscriptionId, cancellationToken);

    public async Task<MercadoPagoSubscription?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.MercadoPagoSubscriptions
            .Where(s => s.UserId == userId && s.Status == "authorized")
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(
        MercadoPagoSubscription subscription,
        CancellationToken cancellationToken = default)
        => await _context.MercadoPagoSubscriptions.AddAsync(subscription, cancellationToken);

    public void Update(MercadoPagoSubscription subscription)
        => _context.MercadoPagoSubscriptions.Update(subscription);
}
