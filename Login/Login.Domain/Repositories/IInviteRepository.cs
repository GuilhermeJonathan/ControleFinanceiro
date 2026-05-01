using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface IInviteRepository
{
    Task<Invite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(Invite invite, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invite>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
