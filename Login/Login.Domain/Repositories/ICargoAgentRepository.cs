using Login.Domain.Entities;

namespace Login.Domain.Repositories;

public interface ICargoAgentRepository
{
    Task<CargoAgentClient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CargoAgentClient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CargoAgentClient cargoAgent, CancellationToken cancellationToken = default);
    void Update(CargoAgentClient cargoAgent);
    void Remove(CargoAgentClient cargoAgent);
}
