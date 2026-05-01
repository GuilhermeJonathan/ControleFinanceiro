using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class InviteRepository : IInviteRepository
{
    private readonly AppDbContext _context;

    public InviteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.Invites.FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

    public async Task AddAsync(Invite invite, CancellationToken cancellationToken = default)
        => await _context.Invites.AddAsync(invite, cancellationToken);

    public async Task<IReadOnlyList<Invite>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Invites
            .Where(i => i.CreatedByUserId == userId)
            .OrderByDescending(i => i.ExpiresAt)
            .ToListAsync(cancellationToken);
}
