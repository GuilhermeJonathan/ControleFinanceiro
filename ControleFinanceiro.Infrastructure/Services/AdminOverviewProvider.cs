using ControleFinanceiro.Application.Admin.Queries.GetAdminOverview;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

/// <inheritdoc />
public class AdminOverviewProvider(AppDbContext db) : IAdminOverviewProvider
{
    public async Task<AdminOverviewDto> GetAsync(CancellationToken ct = default)
    {
        var fx = await db.MoedasParam.Where(m => m.AssessorId == null)
            .ToDictionaryAsync(m => m.Codigo.ToUpperInvariant(), m => m.CotacaoBRL, ct);
        decimal ParaBRL(decimal v, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? v : v * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        var vinculos = await db.VinculosAssessoria
            .Where(v => v.AceitoEm != null && v.RevogadoEm == null)
            .Select(v => new { v.AssessorId, v.ClienteId, v.NomeAssessor })
            .ToListAsync(ct);

        var corretores = await db.VinculosCorretor
            .Where(v => v.AceitoEm != null && v.RevogadoEm == null)
            .Select(v => new { v.AssessorId, v.CorretorId })
            .ToListAsync(ct);

        var consultorias = await db.ConsultoriaConfigs
            .Select(c => new { c.UsuarioId, c.NomeConsultoria })
            .ToListAsync(ct);

        var ativos  = await db.AtivosPatrimoniais.Select(a => new { a.UsuarioId, a.ValorAtual, a.Moeda }).ToListAsync(ct);
        var invests = await db.Investimentos.Select(i => new { i.UsuarioId, i.ValorAtual, i.Moeda }).ToListAsync(ct);

        // AUM por cliente = ativos + investimentos convertidos para BRL.
        var aumPorCliente = new Dictionary<Guid, decimal>();
        foreach (var a in ativos)
            aumPorCliente[a.UsuarioId] = aumPorCliente.GetValueOrDefault(a.UsuarioId) + ParaBRL(a.ValorAtual, a.Moeda);
        foreach (var i in invests)
            aumPorCliente[i.UsuarioId] = aumPorCliente.GetValueOrDefault(i.UsuarioId) + ParaBRL(i.ValorAtual, i.Moeda);

        var nomeConsultoria = consultorias
            .Where(c => !string.IsNullOrWhiteSpace(c.NomeConsultoria))
            .GroupBy(c => c.UsuarioId)
            .ToDictionary(g => g.Key, g => g.First().NomeConsultoria!);

        var nomeVinculo = vinculos
            .Where(v => !string.IsNullOrWhiteSpace(v.NomeAssessor))
            .GroupBy(v => v.AssessorId)
            .ToDictionary(g => g.Key, g => g.First().NomeAssessor!);

        var corretoresPorAssessor = corretores
            .GroupBy(c => c.AssessorId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.CorretorId).Distinct().Count());

        var clientesPorAssessor = vinculos
            .GroupBy(v => v.AssessorId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ClienteId).Distinct().ToList());

        // Assessores conhecidos pela plataforma = quem tem vínculo (assessoria/corretor) ou branding.
        var assessorIds = vinculos.Select(v => v.AssessorId)
            .Concat(corretores.Select(c => c.AssessorId))
            .Concat(consultorias.Select(c => c.UsuarioId))
            .Distinct()
            .ToList();

        var assessorias = assessorIds.Select(id =>
        {
            var clientes = clientesPorAssessor.GetValueOrDefault(id) ?? [];
            var aum = clientes.Sum(cid => aumPorCliente.GetValueOrDefault(cid));
            var nome = nomeConsultoria.GetValueOrDefault(id)
                       ?? nomeVinculo.GetValueOrDefault(id)
                       ?? $"Assessor {id.ToString()[..8]}";
            return new AssessoriaResumoDto(id, nome, clientes.Count,
                corretoresPorAssessor.GetValueOrDefault(id), Math.Round(aum, 2));
        })
        .OrderByDescending(a => a.AumBRL)
        .ToList();

        var tiposAtivoGlobais = await db.TiposAtivoParam.CountAsync(t => t.AssessorId == null, ct);
        var tiposInvGlobais   = await db.TiposInvestimentoParam.CountAsync(t => t.AssessorId == null, ct);
        var qtdMoedas         = await db.MoedasParam.CountAsync(ct);

        return new AdminOverviewDto(
            QtdAssessorias:        assessorias.Count,
            QtdClientes:           vinculos.Select(v => v.ClienteId).Distinct().Count(),
            QtdCorretores:         corretores.Select(c => c.CorretorId).Distinct().Count(),
            AumTotalBRL:           Math.Round(assessorias.Sum(a => a.AumBRL), 2),
            QtdParametrosGlobais:  tiposAtivoGlobais + tiposInvGlobais + qtdMoedas,
            Assessorias:           assessorias);
    }
}
