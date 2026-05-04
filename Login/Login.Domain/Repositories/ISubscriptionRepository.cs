using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task<MercadoPagoSubscription?> GetByMpIdAsync(string mpSubscriptionId, CancellationToken cancellationToken = default);
    Task<MercadoPagoSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(MercadoPagoSubscription subscription, CancellationToken cancellationToken = default);
    void Update(MercadoPagoSubscription subscription);
}
