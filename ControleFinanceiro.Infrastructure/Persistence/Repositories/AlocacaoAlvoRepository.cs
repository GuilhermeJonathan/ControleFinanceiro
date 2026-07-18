using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class AlocacaoAlvoRepository(AppDbContext db) : IAlocacaoAlvoRepository
{
    public async Task<IEnumerable<AlocacaoAlvo>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.AlocacoesAlvo.Where(a => a.UsuarioId == usuarioId).ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<AlocacaoAlvo> alvos, CancellationToken ct = default) =>
        await db.AlocacoesAlvo.AddRangeAsync(alvos, ct);

    public async Task RemoveByUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var atuais = await db.AlocacoesAlvo.Where(a => a.UsuarioId == usuarioId).ToListAsync(ct);
        db.AlocacoesAlvo.RemoveRange(atuais);
    }
}
