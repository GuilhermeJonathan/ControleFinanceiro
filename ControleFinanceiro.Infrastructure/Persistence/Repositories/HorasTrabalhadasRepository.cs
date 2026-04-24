using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class HorasTrabalhadasRepository(AppDbContext context) : IHorasTrabalhadasRepository
{
    public async Task<HorasTrabalhadas?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.HorasTrabalhadas.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<HorasTrabalhadas>> GetByMesAnoAsync(int mes, int ano, CancellationToken cancellationToken = default)
        => await context.HorasTrabalhadas
            .Where(h => h.Mes == mes && h.Ano == ano)
            .OrderBy(h => h.Descricao)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(HorasTrabalhadas horas, CancellationToken cancellationToken = default)
        => await context.HorasTrabalhadas.AddAsync(horas, cancellationToken);

    public void Update(HorasTrabalhadas horas) => context.HorasTrabalhadas.Update(horas);

    public void Delete(HorasTrabalhadas horas) => context.HorasTrabalhadas.Remove(horas);
}
