using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class CartaoCreditoRepository(AppDbContext context) : ICartaoCreditoRepository
{
    public async Task<CartaoCredito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.CartoesCredito.FindAsync([id], cancellationToken);

    public async Task<CartaoCredito?> GetByIdWithParcelasAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.CartoesCredito.Include(c => c.Parcelas).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IEnumerable<CartaoCredito>> GetAllWithParcelasAsync(CancellationToken cancellationToken = default)
        => await context.CartoesCredito.Include(c => c.Parcelas).OrderBy(c => c.Nome).ToListAsync(cancellationToken);

    public async Task AddAsync(CartaoCredito cartao, CancellationToken cancellationToken = default)
        => await context.CartoesCredito.AddAsync(cartao, cancellationToken);

    public void Update(CartaoCredito cartao) => context.CartoesCredito.Update(cartao);
    public void Delete(CartaoCredito cartao) => context.CartoesCredito.Remove(cartao);
}
