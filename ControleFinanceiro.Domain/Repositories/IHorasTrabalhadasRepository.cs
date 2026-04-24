using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IHorasTrabalhadasRepository
{
    Task<HorasTrabalhadas?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<HorasTrabalhadas>> GetByMesAnoAsync(int mes, int ano, CancellationToken cancellationToken = default);
    Task AddAsync(HorasTrabalhadas horas, CancellationToken cancellationToken = default);
    void Update(HorasTrabalhadas horas);
    void Delete(HorasTrabalhadas horas);
}
