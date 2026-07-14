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
    decimal? RentabilidadeAnualPct,
    decimal ValorAplicadoBRL,
    decimal ValorAtualBRL);

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
    IMoedaParamRepository moedaRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetResumoInvestimentosQuery, ResumoInvestimentosDto>
{
    public async Task<ResumoInvestimentosDto> Handle(GetResumoInvestimentosQuery request, CancellationToken cancellationToken)
    {
        var lista = (await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();

        // Câmbio definido pelo assessor em Cadastros → Moedas (CotacaoBRL).
        var fx = (await moedaRepository.GetAllAsync(cancellationToken))
            .ToDictionary(m => m.Codigo.ToUpperInvariant(), m => m.CotacaoBRL);
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

        var investimentosDto = lista.Select(i => new InvestimentoResumoDto(
            i.Id, i.Nome, (int)i.Tipo, i.Moeda.ToString(), i.Corretora, i.Ticker,
            i.ValorAplicado, i.ValorAtual, i.RentabilidadeAnualPct,
            Math.Round(ParaBRL(i.ValorAplicado, i.Moeda), 2),
            Math.Round(ParaBRL(i.ValorAtual, i.Moeda), 2)));

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
