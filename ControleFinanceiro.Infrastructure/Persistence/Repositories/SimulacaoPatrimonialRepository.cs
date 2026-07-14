using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class SimulacaoPatrimonialRepository(AppDbContext db) : ISimulacaoPatrimonialRepository
{
    // Cenarios é owned → o EF já inclui a coleção automaticamente.
    public async Task<IEnumerable<SimulacaoPatrimonial>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.SimulacoesPatrimoniais
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.Favorita)
            .ThenByDescending(x => x.CriadoEm)
            .ToListAsync(ct);

    public Task<SimulacaoPatrimonial?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.SimulacoesPatrimoniais.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(SimulacaoPatrimonial simulacao, CancellationToken ct = default) =>
        await db.SimulacoesPatrimoniais.AddAsync(simulacao, ct);

    public void Update(SimulacaoPatrimonial simulacao) => db.SimulacoesPatrimoniais.Update(simulacao);

    public void Remove(SimulacaoPatrimonial simulacao) => db.SimulacoesPatrimoniais.Remove(simulacao);
}
