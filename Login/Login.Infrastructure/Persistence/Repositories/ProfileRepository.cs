using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly AppDbContext _context;

    public ProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Profiles
            .Include(p => p.Permissions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Profile>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Profiles.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Profile>> GetByUserTypeAsync(UserType userType, CancellationToken cancellationToken = default)
        => await _context.Profiles
            .Where(p => p.UserTypeId == userType)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
        => await _context.Profiles.AddAsync(profile, cancellationToken);

    public void Update(Profile profile)
        => _context.Profiles.Update(profile);

    public void Remove(Profile profile)
        => _context.Profiles.Remove(profile);
}
