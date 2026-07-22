using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;

public record InvestimentoResumoDto(
    Guid Id,
    string Nome,
    int Tipo,
    string? Subclasse,
    string Moeda,
    string? Corretora,
    string? Ticker,
    decimal ValorAplicado,
    decimal ValorAtual,
    decimal? RentabilidadeAnualPct,
    decimal ValorAplicadoBRL,
    decimal ValorAtualBRL,
    DateTime? ValorAtualizadoEm,
    decimal? Quantidade,
    Guid? EstruturaId,
    Guid? ContaId,
    decimal RetornoTotalPct,      // acumulado = (atual - aplicado) / aplicado
    decimal? RetornoAnualPct);    // anualizado (rentab. anual informada, ou derivado do período)

public record TotalInvestPorMoedaDto(string Moeda, decimal TotalAplicado, decimal TotalAtual, int Quantidade);

public record ResumoInvestimentosDto(
    int QtdInvestimentos,
    decimal TotalAplicadoBRL,
    decimal TotalAtualBRL,
    decimal? RentabilidadePct,          // retorno total acumulado = (TotalAtual - TotalAplicado) / TotalAplicado * 100
    bool CambioEstimado,
    IEnumerable<TotalInvestPorMoedaDto> TotaisPorMoeda,
    IEnumerable<InvestimentoResumoDto> Investimentos,
    decimal? RetornoTotalAnualPct = null)  // retorno total anualizado do portfólio (ponderado por valor)
{
    public ResumoInvestimentosDto()
        : this(0, 0m, 0m, null, true, [], [])
    {
    }
}

public record GetResumoInvestimentosQuery : IRequest<ResumoInvestimentosDto>;

public class GetResumoInvestimentosQueryHandler(
    IInvestimentoRepository repository,
    IFxRateResolver fxResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetResumoInvestimentosQuery, ResumoInvestimentosDto>
{
    public async Task<ResumoInvestimentosDto> Handle(GetResumoInvestimentosQuery request, CancellationToken cancellationToken)
    {
        var lista = (await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();

        // Câmbio efetivo do tenant (globais não ocultas + custom do assessor).
        var fx = await fxResolver.GetRatesAsync(cancellationToken);
        decimal ParaBRL(decimal valor, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? valor
            : valor * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        var totaisPorMoeda = lista
            .GroupBy(i => i.Moeda)
            .Select(g => new TotalInvestPorMoedaDto(
                g.Key.ToString(),
                g.Sum(i => i.ValorAplicado),
                g.Sum(i => i.ValorAtual),
                g.Count()))
            .OrderByDescending(t => t.TotalAtual)
            .ToList();

        var totalAplicadoBRL = lista.Sum(i => ParaBRL(i.ValorAplicado, i.Moeda));
        var totalAtualBRL    = lista.Sum(i => ParaBRL(i.ValorAtual, i.Moeda));
        decimal? rentPct     = totalAplicadoBRL > 0
            ? Math.Round((totalAtualBRL - totalAplicadoBRL) / totalAplicadoBRL * 100, 2)
            : null;

        var investimentosDto = lista.Select(i =>
        {
            var retTotal = i.ValorAplicado > 0
                ? Math.Round((i.ValorAtual - i.ValorAplicado) / i.ValorAplicado * 100, 2) : 0m;
            var retAnual = i.RentabilidadeAnualPct ?? Anualizar(i.ValorAplicado, i.ValorAtual, i.CriadoEm);
            return new InvestimentoResumoDto(
                i.Id, i.Nome, (int)i.Tipo, i.Subclasse, i.Moeda.ToString(), i.Corretora, i.Ticker,
                i.ValorAplicado, i.ValorAtual, i.RentabilidadeAnualPct,
                Math.Round(ParaBRL(i.ValorAplicado, i.Moeda), 2),
                Math.Round(ParaBRL(i.ValorAtual, i.Moeda), 2),
                i.ValorAtualizadoEm, i.Quantidade, i.EstruturaId, i.ContaId,
                retTotal, retAnual);
        }).ToList();

        // Retorno total anualizado do portfólio = média dos anuais ponderada pelo valor atual (BRL).
        var comAnual = investimentosDto.Where(d => d.RetornoAnualPct != null && d.ValorAtualBRL > 0).ToList();
        var pesoAnual = comAnual.Sum(d => d.ValorAtualBRL);
        decimal? retTotalAnual = pesoAnual > 0
            ? Math.Round(comAnual.Sum(d => d.ValorAtualBRL * d.RetornoAnualPct!.Value) / pesoAnual, 2) : null;

        return new ResumoInvestimentosDto(
            lista.Count,
            Math.Round(totalAplicadoBRL, 2),
            Math.Round(totalAtualBRL, 2),
            rentPct,
            CambioEstimado: true,
            totaisPorMoeda,
            investimentosDto,
            retTotalAnual);
    }

    /// <summary>Anualiza o retorno acumulado pelo período desde o cadastro (guarda contra períodos curtos).</summary>
    private static decimal? Anualizar(decimal aplicado, decimal atual, DateTime criadoEm)
    {
        if (aplicado <= 0 || atual <= 0) return null;
        var dias = (DateTime.UtcNow - criadoEm).TotalDays;
        if (dias < 180) return null; // < 6 meses → anualizar distorce demais
        var anos = dias / 365.25;
        var anual = Math.Pow((double)(atual / aplicado), 1.0 / anos) - 1.0;
        return Math.Round((decimal)(anual * 100), 2);
    }
}
