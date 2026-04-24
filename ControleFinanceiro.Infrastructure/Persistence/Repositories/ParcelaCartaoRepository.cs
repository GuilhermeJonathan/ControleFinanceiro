using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class ParcelaCartaoRepository(AppDbContext context) : IParcelaCartaoRepository
{
    public async Task<ParcelaCartao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ParcelasCartao.FindAsync([id], cancellationToken);

    public async Task AddAsync(ParcelaCartao parcela, CancellationToken cancellationToken = default)
        => await context.ParcelasCartao.AddAsync(parcela, cancellationToken);

    public void Update(ParcelaCartao parcela) => context.ParcelasCartao.Update(parcela);

    public void Delete(ParcelaCartao parcela) => context.ParcelasCartao.Remove(parcela);
}
