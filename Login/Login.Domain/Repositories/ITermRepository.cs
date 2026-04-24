using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface ITermRepository
{
    Task<bool> HasAcceptedAsync(Guid userId, string termName, CancellationToken cancellationToken = default);
    Task AddAsync(AcceptedTerm term, CancellationToken cancellationToken = default);
}
