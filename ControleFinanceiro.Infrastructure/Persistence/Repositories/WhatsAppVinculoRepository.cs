using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class WhatsAppVinculoRepository(AppDbContext db) : IWhatsAppVinculoRepository
{
    public Task<WhatsAppVinculo?> GetByPhoneAsync(string phone, CancellationToken ct = default) =>
        db.WhatsAppVinculos.FirstOrDefaultAsync(
            v => v.PhoneNumber == WhatsAppVinculo.Normalize(phone), ct);

    public Task<WhatsAppVinculo?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        db.WhatsAppVinculos.FirstOrDefaultAsync(v => v.UserId == userId, ct);

    public async Task AddAsync(WhatsAppVinculo vinculo, CancellationToken ct = default) =>
        await db.WhatsAppVinculos.AddAsync(vinculo, ct);

    public void Remove(WhatsAppVinculo vinculo) =>
        db.WhatsAppVinculos.Remove(vinculo);
}
