using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class WhatsAppVinculoRepository(AppDbContext db) : IWhatsAppVinculoRepository
{
    public Task<WhatsAppVinculo?> GetByPhoneAsync(string phone, CancellationToken ct = default)
    {
        var normalized   = WhatsAppVinculo.Normalize(phone);
        var alternatives = PhoneAlternatives(normalized);
        return db.WhatsAppVinculos.FirstOrDefaultAsync(
            v => alternatives.Contains(v.PhoneNumber), ct);
    }

    /// <summary>
    /// Gera variações com/sem o dígito 9 para números brasileiros.
    /// Ex: 5561981914325 ↔ 556181914325
    /// </summary>
    private static string[] PhoneAlternatives(string normalized)
    {
        var list = new List<string> { normalized };

        // Brasil: 55 + 2 dígitos DDD + 9 + 8 dígitos = 13 dígitos
        if (normalized.StartsWith("55") && normalized.Length == 13)
            list.Add(normalized[..4] + normalized[5..]); // remove o 9

        // Brasil: 55 + 2 dígitos DDD + 8 dígitos = 12 dígitos
        else if (normalized.StartsWith("55") && normalized.Length == 12)
            list.Add(normalized[..4] + "9" + normalized[4..]); // adiciona o 9

        return [.. list];
    }

    public Task<WhatsAppVinculo?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        db.WhatsAppVinculos.FirstOrDefaultAsync(v => v.UserId == userId, ct);

    public async Task AddAsync(WhatsAppVinculo vinculo, CancellationToken ct = default) =>
        await db.WhatsAppVinculos.AddAsync(vinculo, ct);

    public void Remove(WhatsAppVinculo vinculo) =>
        db.WhatsAppVinculos.Remove(vinculo);
}
