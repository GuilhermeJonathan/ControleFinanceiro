using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoPatrimonio;

/// <summary>Patrimônio projetado (BRL) em um mês do horizonte: bens valorizados − dívidas amortizadas.</summary>
public record PontoProjecaoPatrimonioDto(int MesOffset, decimal BensBRL, decimal DividasBRL, decimal PatrimonioLiquidoBRL);

public record ProjecaoPatrimonioDto(
    int HorizonteMeses,
    bool CambioEstimado,
    decimal PatrimonioInicialBRL,
    decimal PatrimonioFinalBRL,
    int? MesesQuitacaoDividas,
    IEnumerable<PontoProjecaoPatrimonioDto> Pontos)
{
    public ProjecaoPatrimonioDto() : this(0, true, 0m, 0m, null, []) { }
}

/// <summary>
/// Projeta o patrimônio líquido mês a mês para comparar com a quitação das dívidas:
/// os bens crescem pela valorização anual (composta ao mês) e as dívidas amortizam (Price),
/// no mesmo horizonte da projeção de dívidas. Câmbio ESTUB (CotacaoBRL definido pelo assessor).
/// Espelha a composição do resumo (Bens − Dívidas); Investimentos ficam à parte.
/// </summary>
public record GetProjecaoPatrimonioQuery(int? Meses = null) : IRequest<ProjecaoPatrimonioDto>;

public class GetProjecaoPatrimonioQueryHandler(
    IAtivoPatrimonialRepository ativoRepository,
    IPassivoPatrimonialRepository passivoRepository,
    IFxRateResolver fxResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetProjecaoPatrimonioQuery, ProjecaoPatrimonioDto>
{
    private const int HorizonteMax = 360;    // 30 anos
    private const int HorizontePadrao = 120; // 10 anos quando não há cronograma

    public async Task<ProjecaoPatrimonioDto> Handle(GetProjecaoPatrimonioQuery request, CancellationToken cancellationToken)
    {
        var ativos = (await ativoRepository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();
        var passivos = (await passivoRepository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();
        if (ativos.Count == 0 && passivos.Count == 0)
            return new ProjecaoPatrimonioDto(0, true, 0m, 0m, null, []);

        // Câmbio definido pelo assessor em Cadastros → Moedas (CotacaoBRL).
        var fxMap = await fxResolver.GetRatesAsync(cancellationToken);
        decimal Fx(MoedaPatrimonio m) => m == MoedaPatrimonio.BRL ? 1m
            : (fxMap.TryGetValue(m.ToString(), out var r) && r > 0 ? r : 1m);

        var comCronograma = passivos.Where(p => p.PrazoMeses is > 0).ToList();
        var horizonte = request.Meses
            ?? (comCronograma.Count > 0 ? comCronograma.Max(p => p.PrazoMeses!.Value) : HorizontePadrao);
        horizonte = Math.Clamp(horizonte, 1, HorizonteMax);

        // ── Dívidas por mês (Price; sem cronograma → saldo constante) ──
        var dividasPorMes = new decimal[horizonte + 1];
        foreach (var p in passivos)
        {
            var fx = Fx(p.Moeda);
            var saldo = p.Valor;
            var n = p.PrazoMeses ?? 0;
            var i = (p.TaxaJurosAnualPct ?? 0m) / 100m / 12m;
            var parcela = n > 0
                ? (i > 0
                    ? saldo * i / (1m - (decimal)Math.Pow((double)(1m + i), -n))
                    : saldo / n)
                : 0m;

            for (var mes = 0; mes <= horizonte; mes++)
            {
                dividasPorMes[mes] += Math.Max(saldo, 0m) * fx;
                if (n > 0 && mes < horizonte && saldo > 0m)
                    saldo = Math.Max(saldo - (parcela - saldo * i), 0m);
            }
        }

        // ── Bens por mês (valorização anual composta ao mês) ──
        var bensPorMes = new decimal[horizonte + 1];
        foreach (var a in ativos)
        {
            var baseBRL = a.ValorAtual * Fx(a.Moeda);
            var g = (a.ValorizacaoAnualPct ?? 0m) / 100m / 12m;
            for (var mes = 0; mes <= horizonte; mes++)
                bensPorMes[mes] += baseBRL * (decimal)Math.Pow((double)(1m + g), mes);
        }

        var pontos = new List<PontoProjecaoPatrimonioDto>(horizonte + 1);
        int? mesesQuitacao = null;
        for (var mes = 0; mes <= horizonte; mes++)
        {
            var bens = Math.Round(bensPorMes[mes], 2);
            var div = Math.Round(dividasPorMes[mes], 2);
            pontos.Add(new PontoProjecaoPatrimonioDto(mes, bens, div, Math.Round(bens - div, 2)));
            if (mesesQuitacao is null && mes > 0 && div <= 0m && dividasPorMes[0] > 0m)
                mesesQuitacao = mes;
        }

        return new ProjecaoPatrimonioDto(
            HorizonteMeses: horizonte,
            CambioEstimado: true,
            PatrimonioInicialBRL: pontos[0].PatrimonioLiquidoBRL,
            PatrimonioFinalBRL: pontos[^1].PatrimonioLiquidoBRL,
            MesesQuitacaoDividas: mesesQuitacao,
            Pontos: pontos);
    }
}
