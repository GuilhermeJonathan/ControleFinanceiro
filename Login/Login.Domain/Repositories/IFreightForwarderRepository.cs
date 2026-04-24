using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface IFreightForwarderRepository
{
    Task<FreightForwarder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FreightForwarder?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FreightForwarder>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(FreightForwarder freightForwarder, CancellationToken cancellationToken = default);
    void Update(FreightForwarder freightForwarder);
    void Remove(FreightForwarder freightForwarder);
}
