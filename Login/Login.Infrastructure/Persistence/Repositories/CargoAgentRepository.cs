using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class CargoAgentRepository : ICargoAgentRepository
{
    private readonly AppDbContext _context;

    public CargoAgentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CargoAgentClient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.CargoAgentClients
            .Include(c => c.Permissions)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CargoAgentClient>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.CargoAgentClients
            .Include(c => c.Permissions)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CargoAgentClient cargoAgent, CancellationToken cancellationToken = default)
        => await _context.CargoAgentClients.AddAsync(cargoAgent, cancellationToken);

    public void Update(CargoAgentClient cargoAgent)
        => _context.CargoAgentClients.Update(cargoAgent);

    public void Remove(CargoAgentClient cargoAgent)
        => _context.CargoAgentClients.Remove(cargoAgent);
}
