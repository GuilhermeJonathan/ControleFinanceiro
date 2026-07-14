using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ConsultoriaConfigRepository(AppDbContext db) : IConsultoriaConfigRepository
{
    public Task<ConsultoriaConfig?> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        db.ConsultoriaConfigs.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId, ct);

    public async Task AddAsync(ConsultoriaConfig config, CancellationToken ct = default) =>
        await db.ConsultoriaConfigs.AddAsync(config, ct);

    public void Update(ConsultoriaConfig config) => db.ConsultoriaConfigs.Update(config);
}
