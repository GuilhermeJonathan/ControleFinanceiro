using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IParcelaCartaoRepository
{
    Task<ParcelaCartao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ParcelaCartao parcela, CancellationToken cancellationToken = default);
    void Update(ParcelaCartao parcela);
    void Delete(ParcelaCartao parcela);
}
