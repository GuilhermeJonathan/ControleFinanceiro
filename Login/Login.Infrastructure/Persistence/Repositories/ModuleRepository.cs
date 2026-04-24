using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly AppDbContext _context;

    public ModuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Modules
            .Include(m => m.Functions)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Module>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Modules.Include(m => m.Functions).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Module>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        => await _context.Modules.Where(m => m.IsActive).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Module>> GetByProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var moduleIds = await _context.Permissions
            .Where(p => p.ProfileId == profileId)
            .Select(p => p.ModuleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.Modules
            .Where(m => moduleIds.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }
}
