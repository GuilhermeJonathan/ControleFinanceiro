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

    public async Task<UserContato?> GetContatoAsync(Guid userId, CancellationToken ct = default)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT \"Name\", \"Email\", \"AvatarUrl\" FROM \"Users\" WHERE \"Id\" = @id LIMIT 1";
        var p = cmd.CreateParameter();
        p.ParameterName = "id";
        p.Value = userId;
        cmd.Parameters.Add(p);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return new UserContato(
            reader.IsDBNull(0) ? null : reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2));
    }
}
