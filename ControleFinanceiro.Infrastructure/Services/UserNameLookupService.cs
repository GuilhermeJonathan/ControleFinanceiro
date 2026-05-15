using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class UserNameLookupService(AppDbContext db) : IUserNameLookup
{
    public async Task<string?> GetNomeAsync(Guid userId, CancellationToken ct = default)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT \"Name\" FROM \"Users\" WHERE \"Id\" = @id LIMIT 1";
        var p = cmd.CreateParameter();
        p.ParameterName = "id";
        p.Value = userId;
        cmd.Parameters.Add(p);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result as string;
    }
}
