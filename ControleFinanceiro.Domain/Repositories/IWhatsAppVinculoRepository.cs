using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface IWhatsAppVinculoRepository
{
    Task<WhatsAppVinculo?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task<WhatsAppVinculo?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<WhatsAppVinculo>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(WhatsAppVinculo vinculo, CancellationToken ct = default);
    void Remove(WhatsAppVinculo vinculo);
}
