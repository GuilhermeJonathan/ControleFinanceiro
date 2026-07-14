using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;

/// <summary>Saldo devedor total (BRL) projetado em um mês do horizonte.</summary>
public record PontoProjecaoDto(int MesOffset, decimal SaldoBRL);

public record ProjecaoDividasDto(
    decimal SaldoInicialBRL,
    int HorizonteMeses,
    bool CambioEstimado,
    IEnumerable<PontoProjecaoDto> Pontos)
{
    public ProjecaoDividasDto() : this(0m, 0, true, []) { }
}

/// <summary>
/// Projeta a quitação das dívidas mês a mês (tabela Price por dívida, somada).
/// Dívidas sem cronograma (PrazoMeses nulo) permanecem com saldo constante.
/// Câmbio ESTUB — ver FxStub. Meses opcional limita/estende o horizonte.
/// </summary>
public record GetProjecaoDividasQuery(int? Meses = null) : IRequest<ProjecaoDividasDto>;

public class GetProjecaoDividasQueryHandler(
    IPassivoPatrimonialRepository repository,
    IMoedaParamRepository moedaRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetProjecaoDividasQuery, ProjecaoDividasDto>
{
    private const int HorizonteMax = 360;   // 30 anos
    private const int HorizontePadrao = 120; // 10 anos quando não há cronograma

    public async Task<ProjecaoDividasDto> Handle(GetProjecaoDividasQuery request, CancellationToken cancellationToken)
    {
        var passivos = (await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();
        if (passivos.Count == 0)
            return new ProjecaoDividasDto(0m, 0, true, []);

        // Câmbio definido pelo assessor em Cadastros → Moedas (CotacaoBRL).
        var fxMap = (await moedaRepository.GetAllAsync(cancellationToken))
            .ToDictionary(m => m.Codigo.ToUpperInvariant(), m => m.CotacaoBRL);

        var comCronograma = passivos.Where(p => p.PrazoMeses is > 0).ToList();
        var horizonte = request.Meses
            ?? (comCronograma.Count > 0 ? comCronograma.Max(p => p.PrazoMeses!.Value) : HorizontePadrao);
        horizonte = Math.Clamp(horizonte, 1, HorizonteMax);

        // Saldo (BRL) de cada dívida em cada mês do horizonte.
        var saldosPorMes = new decimal[horizonte + 1];
        foreach (var p in passivos)
        {
            var fx = p.Moeda == MoedaPatrimonio.BRL ? 1m
                : (fxMap.TryGetValue(p.Moeda.ToString(), out var r) && r > 0 ? r : 1m);
            var saldo = p.Valor;
            var n = p.PrazoMeses ?? 0;
            var i = (p.TaxaJurosAnualPct ?? 0m) / 100m / 12m;

            // Parcela fixa (Price). Sem cronograma → saldo constante.
            decimal parcela = 0m;
            if (n > 0)
            {
                parcela = i > 0
                    ? saldo * i / (1m - (decimal)Math.Pow((double)(1m + i), -n))
                    : saldo / n;
            }

            for (var mes = 0; mes <= horizonte; mes++)
            {
                saldosPorMes[mes] += Math.Max(saldo, 0m) * fx;
                if (n > 0 && mes < horizonte && saldo > 0m)
                {
                    var juros = saldo * i;
                    saldo = Math.Max(saldo - (parcela - juros), 0m);
                }
            }
        }

        var pontos = new List<PontoProjecaoDto>(horizonte + 1);
        for (var mes = 0; mes <= horizonte; mes++)
            pontos.Add(new PontoProjecaoDto(mes, Math.Round(saldosPorMes[mes], 2)));

        return new ProjecaoDividasDto(
            SaldoInicialBRL: pontos[0].SaldoBRL,
            HorizonteMeses: horizonte,
            CambioEstimado: true,
            Pontos: pontos);
    }
}
