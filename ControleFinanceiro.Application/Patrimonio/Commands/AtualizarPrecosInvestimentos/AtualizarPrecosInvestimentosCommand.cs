using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.AtualizarPrecosInvestimentos;

public record AtualizarPrecosResult(int Atualizados, bool Pulado);

/// <summary>
/// Atualiza o valor atual dos investimentos do usuário efetivo que têm ticker, via preço de mercado,
/// e grava o histórico de preço. Forcar=false respeita a guarda de frescor (usado pelo job).
/// </summary>
public record AtualizarPrecosInvestimentosCommand(bool Forcar = false) : IRequest<AtualizarPrecosResult>;

public class AtualizarPrecosInvestimentosCommandHandler(
    IInvestimentoRepository investimentoRepo,
    IAssetPriceService priceService,                  // brapi (ativos nacionais / B3)
    IExteriorAssetPriceService exteriorPriceService,  // yahoo (ativos do exterior)
    ITipoInvestimentoParamRepository tipoRepo,
    IPrecoAtivoHistoricoRepository historicoRepo,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AtualizarPrecosInvestimentosCommand, AtualizarPrecosResult>
{
    private static readonly TimeSpan Frescor = TimeSpan.FromHours(3);

    public async Task<AtualizarPrecosResult> Handle(AtualizarPrecosInvestimentosCommand request, CancellationToken ct)
    {
        var investimentos = (await investimentoRepo.GetByUsuarioAsync(currentUser.UserId, ct))
            .Where(i => !string.IsNullOrWhiteSpace(i.Ticker))
            .ToList();
        if (investimentos.Count == 0) return new AtualizarPrecosResult(0, false);

        if (!request.Forcar)
        {
            var maisRecente = investimentos
                .Where(i => i.ValorAtualizadoEm.HasValue)
                .Select(i => i.ValorAtualizadoEm!.Value)
                .DefaultIfEmpty()
                .Max();
            if (maisRecente != default && DateTime.UtcNow - maisRecente < Frescor)
                return new AtualizarPrecosResult(0, true);
        }

        // Tipos marcados "Exterior" → cotação via Yahoo; demais → brapi (B3).
        var tipos       = await tipoRepo.GetGlobaisAsync(ct);
        var exteriorIds = tipos.Where(t => t.Exterior).Select(t => t.Id).ToHashSet();
        bool EhExterior(Domain.Entities.Investimento i) => exteriorIds.Contains((int)i.Tipo);

        string Norm(Domain.Entities.Investimento i) => i.Ticker!.Trim().ToUpperInvariant();
        var tickersNac = investimentos.Where(i => !EhExterior(i)).Select(Norm).Distinct().ToList();
        var tickersExt = investimentos.Where(EhExterior).Select(Norm).Distinct().ToList();

        var precos = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        if (tickersNac.Count > 0)
            foreach (var kv in await priceService.GetPricesAsync(tickersNac, ct)) precos[kv.Key] = kv.Value;
        if (tickersExt.Count > 0)
            foreach (var kv in await exteriorPriceService.GetPricesAsync(tickersExt, ct)) precos[kv.Key] = kv.Value;

        if (precos.Count == 0) return new AtualizarPrecosResult(0, false);

        var extSet = tickersExt.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var atualizados = 0;
        foreach (var ticker in tickersNac.Concat(tickersExt))
        {
            if (!precos.TryGetValue(ticker, out var preco)) continue;
            foreach (var inv in investimentos.Where(i => string.Equals(Norm(i), ticker, StringComparison.OrdinalIgnoreCase)))
            {
                if (inv.AtualizarValorAutomatico(preco)) atualizados++;
            }
            var fonte = extSet.Contains(ticker) ? "yahoo" : "brapi.dev";
            await historicoRepo.AddAsync(new PrecoAtivoHistorico(ticker, preco, fonte), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new AtualizarPrecosResult(atualizados, false);
    }
}
