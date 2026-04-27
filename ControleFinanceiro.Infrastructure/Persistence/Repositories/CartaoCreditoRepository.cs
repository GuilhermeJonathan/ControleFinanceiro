using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class CartaoCreditoRepository(AppDbContext context) : ICartaoCreditoRepository
{
    public async Task<CartaoCredito?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.CartoesCredito.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId, cancellationToken);

    public async Task<CartaoCredito?> GetByIdWithParcelasAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.CartoesCredito.Include(c => c.Parcelas).FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<CartaoCredito>> GetAllWithParcelasAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.CartoesCredito.Include(c => c.Parcelas).Where(c => c.UsuarioId == usuarioId).OrderBy(c => c.Nome).ToListAsync(cancellationToken);

    public async Task AddAsync(CartaoCredito cartao, CancellationToken cancellationToken = default)
        => await context.CartoesCredito.AddAsync(cartao, cancellationToken);

    public void Update(CartaoCredito cartao) => context.CartoesCredito.Update(cartao);
    public void Delete(CartaoCredito cartao) => context.CartoesCredito.Remove(cartao);
}
