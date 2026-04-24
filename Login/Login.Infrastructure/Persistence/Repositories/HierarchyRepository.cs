using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class HierarchyRepository : IHierarchyRepository
{
    private readonly AppDbContext _context;

    public HierarchyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Hierarchy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Hierarchies
            .Include(h => h.Companies)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Hierarchy>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Hierarchies.Include(h => h.Companies).ToListAsync(cancellationToken);

    public async Task AddAsync(Hierarchy hierarchy, CancellationToken cancellationToken = default)
        => await _context.Hierarchies.AddAsync(hierarchy, cancellationToken);

    public void Update(Hierarchy hierarchy)
        => _context.Hierarchies.Update(hierarchy);

    public void Remove(Hierarchy hierarchy)
        => _context.Hierarchies.Remove(hierarchy);
}
