using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface IModuleRepository
{
    Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Module>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Module>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Module>> GetByProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}
