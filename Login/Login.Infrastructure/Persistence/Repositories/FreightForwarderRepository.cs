using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class FreightForwarderRepository : IFreightForwarderRepository
{
    private readonly AppDbContext _context;

    public FreightForwarderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FreightForwarder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.FreightForwarders
            .Include(f => f.Permissions)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<FreightForwarder?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
        => await _context.FreightForwarders
            .FirstOrDefaultAsync(f => f.Document == document, cancellationToken);

    public async Task<IReadOnlyList<FreightForwarder>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.FreightForwarders.ToListAsync(cancellationToken);

    public async Task AddAsync(FreightForwarder freightForwarder, CancellationToken cancellationToken = default)
        => await _context.FreightForwarders.AddAsync(freightForwarder, cancellationToken);

    public void Update(FreightForwarder freightForwarder)
        => _context.FreightForwarders.Update(freightForwarder);

    public void Remove(FreightForwarder freightForwarder)
        => _context.FreightForwarders.Remove(freightForwarder);
}
