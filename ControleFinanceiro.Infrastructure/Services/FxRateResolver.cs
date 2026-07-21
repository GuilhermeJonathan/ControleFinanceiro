using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;

namespace ControleFinanceiro.Infrastructure.Services;

/// <inheritdoc />
public class FxRateResolver(
    IAssessoriaOwnerResolver ownerResolver,
    IMoedaParamRepository moedaRepo,
    IParametroOcultoRepository ocultoRepo)
    : IFxRateResolver
{
    public async Task<IReadOnlyDictionary<string, decimal>> GetRatesAsync(CancellationToken ct = default)
    {
        var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var owner = await ownerResolver.ResolveOwnerAsync(ct);

        if (owner is null)
        {
            foreach (var m in await moedaRepo.GetGlobaisAsync(ct))
                dict[m.Codigo.ToUpperInvariant()] = m.CotacaoBRL;
            return dict;
        }

        var todos   = await moedaRepo.GetGlobaisEDoAssessorAsync(owner.Value, ct);
        var ocultos = (await ocultoRepo.GetIdsOcultosAsync(owner.Value, TipoParametroCatalogo.Moeda, ct)).ToHashSet();

        // Globais não ocultas primeiro; custom depois (sobrescreve global de mesmo código).
        foreach (var m in todos.Where(x => x.AssessorId is null && !ocultos.Contains(x.Id)))
            dict[m.Codigo.ToUpperInvariant()] = m.CotacaoBRL;
        foreach (var m in todos.Where(x => x.AssessorId is not null))
            dict[m.Codigo.ToUpperInvariant()] = m.CotacaoBRL;

        return dict;
    }
}
