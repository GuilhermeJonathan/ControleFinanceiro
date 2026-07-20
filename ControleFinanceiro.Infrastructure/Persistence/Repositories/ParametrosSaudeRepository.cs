using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ParametrosSaudeRepository(AppDbContext db) : IParametrosSaudeRepository
{
    public Task<ParametrosSaude?> GetByAssessorAsync(Guid assessorId, CancellationToken ct = default) =>
        db.ParametrosSaude.FirstOrDefaultAsync(p => p.AssessorId == assessorId, ct);

    public async Task AddAsync(ParametrosSaude parametros, CancellationToken ct = default) =>
        await db.ParametrosSaude.AddAsync(parametros, ct);

    public void Update(ParametrosSaude parametros) => db.ParametrosSaude.Update(parametros);
}
