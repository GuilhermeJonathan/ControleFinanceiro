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
    IAssetPriceService priceService,
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

        var tickers = investimentos.Select(i => i.Ticker!.Trim().ToUpperInvariant()).Distinct().ToList();
        var precos = await priceService.GetPricesAsync(tickers, ct);
        if (precos.Count == 0) return new AtualizarPrecosResult(0, false);

        var atualizados = 0;
        foreach (var ticker in tickers)
        {
            if (!precos.TryGetValue(ticker, out var preco)) continue;
            foreach (var inv in investimentos.Where(i => string.Equals(i.Ticker!.Trim(), ticker, StringComparison.OrdinalIgnoreCase)))
            {
                inv.AtualizarValorAutomatico(preco);
                atualizados++;
            }
            await historicoRepo.AddAsync(new PrecoAtivoHistorico(ticker, preco, "brapi.dev"), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new AtualizarPrecosResult(atualizados, false);
    }
}
