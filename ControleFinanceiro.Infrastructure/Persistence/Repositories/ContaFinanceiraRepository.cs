using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ContaFinanceiraRepository(AppDbContext db) : IContaFinanceiraRepository
{
    public Task<List<ContaFinanceira>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        db.ContasFinanceiras.Where(c => c.UsuarioId == usuarioId).OrderBy(c => c.CriadoEm).ToListAsync(ct);

    public Task<ContaFinanceira?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(ContaFinanceira entity, CancellationToken ct = default) =>
        await db.ContasFinanceiras.AddAsync(entity, ct);

    public void Remove(ContaFinanceira entity) => db.ContasFinanceiras.Remove(entity);
}
