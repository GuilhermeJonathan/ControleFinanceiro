using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;

public record InvestimentoResumoDto(
    Guid Id,
    string Nome,
    int Tipo,
    string Moeda,
    string? Corretora,
    string? Ticker,
    decimal ValorAplicado,
    decimal ValorAtual,
    decimal? RentabilidadeAnualPct);

public record TotalInvestPorMoedaDto(string Moeda, decimal TotalAplicado, decimal TotalAtual, int Quantidade);

public record ResumoInvestimentosDto(
    int QtdInvestimentos,
    decimal TotalAplicadoBRL,
    decimal TotalAtualBRL,
    decimal? RentabilidadePct,          // (TotalAtual - TotalAplicado) / TotalAplicado * 100
    bool CambioEstimado,
    IEnumerable<TotalInvestPorMoedaDto> TotaisPorMoeda,
    IEnumerable<InvestimentoResumoDto> Investimentos)
{
    public ResumoInvestimentosDto()
        : this(0, 0m, 0m, null, true, [], [])
    {
    }
}

public record GetResumoInvestimentosQuery : IRequest<ResumoInvestimentosDto>;

public class GetResumoInvestimentosQueryHandler(
    IInvestimentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetResumoInvestimentosQuery, ResumoInvestimentosDto>
{
    private static readonly Dictionary<MoedaPatrimonio, decimal> FxStub = new()
    {
        [MoedaPatrimonio.BRL] = 1.00m,
        [MoedaPatrimonio.USD] = 5.40m,
        [MoedaPatrimonio.EUR] = 5.90m,
        [MoedaPatrimonio.CHF] = 6.10m,
        [MoedaPatrimonio.GBP] = 6.90m,
    };

    public async Task<ResumoInvestimentosDto> Handle(GetResumoInvestimentosQuery request, CancellationToken cancellationToken)
    {
        var lista = (await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();

        var totaisPorMoeda = lista
            .GroupBy(i => i.Moeda)
            .Select(g => new TotalInvestPorMoedaDto(
                g.Key.ToString(),
                g.Sum(i => i.ValorAplicado),
                g.Sum(i => i.ValorAtual),
                g.Count()))
            .OrderByDescending(t => t.TotalAtual)
            .ToList();

        var fx = FxStub;
        var totalAplicadoBRL = lista.Sum(i => i.ValorAplicado * fx.GetValueOrDefault(i.Moeda, 1m));
        var totalAtualBRL    = lista.Sum(i => i.ValorAtual    * fx.GetValueOrDefault(i.Moeda, 1m));
        decimal? rentPct     = totalAplicadoBRL > 0
            ? Math.Round((totalAtualBRL - totalAplicadoBRL) / totalAplicadoBRL * 100, 2)
            : null;

        var investimentosDto = lista.Select(i => new InvestimentoResumoDto(
            i.Id, i.Nome, (int)i.Tipo, i.Moeda.ToString(), i.Corretora, i.Ticker,
            i.ValorAplicado, i.ValorAtual, i.RentabilidadeAnualPct));

        return new ResumoInvestimentosDto(
            lista.Count,
            Math.Round(totalAplicadoBRL, 2),
            Math.Round(totalAtualBRL, 2),
            rentPct,
            CambioEstimado: true,
            totaisPorMoeda,
            investimentosDto);
    }
}
