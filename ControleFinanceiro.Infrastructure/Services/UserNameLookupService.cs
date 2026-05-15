using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class UserNameLookupService(AppDbContext db) : IUserNameLookup
{
    public async Task<string?> GetNomeAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await db.Database
            .SqlQueryRaw<string>(
                "SELECT \"Name\" AS \"Value\" FROM \"Users\" WHERE \"Id\" = {0} LIMIT 1",
                userId)
            .FirstOrDefaultAsync(ct);

        return result;
    }
}
