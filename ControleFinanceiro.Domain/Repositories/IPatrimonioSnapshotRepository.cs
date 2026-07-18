using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IPatrimonioSnapshotRepository
{
    Task<PatrimonioSnapshot?> GetByUsuarioMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default);
    Task<IEnumerable<PatrimonioSnapshot>> GetByUsuarioAsync(Guid usuarioId, int meses, CancellationToken ct = default);
    Task AddAsync(PatrimonioSnapshot snapshot, CancellationToken ct = default);
    void Update(PatrimonioSnapshot snapshot);
}
