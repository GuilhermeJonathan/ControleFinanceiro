using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Profile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Profile>> GetByUserTypeAsync(UserType userType, CancellationToken cancellationToken = default);
    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);
    void Update(Profile profile);
    void Remove(Profile profile);
}
