using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface IHierarchyRepository
{
    Task<Hierarchy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Hierarchy>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Hierarchy hierarchy, CancellationToken cancellationToken = default);
    void Update(Hierarchy hierarchy);
    void Remove(Hierarchy hierarchy);
}
