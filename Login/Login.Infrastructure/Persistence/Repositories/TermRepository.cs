using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Login.Infrastructure.Persistence.Repositories;

public class TermRepository : ITermRepository
{
    private readonly AppDbContext _context;

    public TermRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasAcceptedAsync(Guid userId, string termName, CancellationToken cancellationToken = default)
        => await _context.AcceptedTerms
            .AnyAsync(t => t.UserId == userId && t.TermName == termName, cancellationToken);

    public async Task AddAsync(AcceptedTerm term, CancellationToken cancellationToken = default)
        => await _context.AcceptedTerms.AddAsync(term, cancellationToken);
}
