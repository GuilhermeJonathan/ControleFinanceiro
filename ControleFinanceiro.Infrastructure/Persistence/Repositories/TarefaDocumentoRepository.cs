using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class TarefaDocumentoRepository(AppDbContext db) : ITarefaDocumentoRepository
{
    public Task<List<TarefaDocumento>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default) =>
        db.TarefasDocumento.Where(x => x.ClienteId == clienteId).OrderByDescending(x => x.CriadoEm).ToListAsync(ct);

    public Task<List<TarefaDocumento>> GetByAssessorClienteAsync(Guid assessorId, Guid clienteId, CancellationToken ct = default) =>
        db.TarefasDocumento.Where(x => x.AssessorId == assessorId && x.ClienteId == clienteId)
            .OrderByDescending(x => x.CriadoEm).ToListAsync(ct);

    public Task<TarefaDocumento?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TarefasDocumento.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TarefaDocumento tarefa, CancellationToken ct = default) =>
        await db.TarefasDocumento.AddAsync(tarefa, ct);

    public void Remove(TarefaDocumento tarefa) => db.TarefasDocumento.Remove(tarefa);
}
