using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IHorasTrabalhadasRepository
{
    Task<HorasTrabalhadas?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HorasTrabalhadas>> GetByMesAnoAsync(int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default);
    Task AddAsync(HorasTrabalhadas horas, CancellationToken cancellationToken = default);
    void Update(HorasTrabalhadas horas);
    void Delete(HorasTrabalhadas horas);
}
