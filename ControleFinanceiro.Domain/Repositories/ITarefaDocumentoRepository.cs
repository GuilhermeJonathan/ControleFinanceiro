using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ITarefaDocumentoRepository
{
    Task<List<TarefaDocumento>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<List<TarefaDocumento>> GetByAssessorClienteAsync(Guid assessorId, Guid clienteId, CancellationToken ct = default);
    Task<TarefaDocumento?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TarefaDocumento tarefa, CancellationToken ct = default);
    void Remove(TarefaDocumento tarefa);
}
