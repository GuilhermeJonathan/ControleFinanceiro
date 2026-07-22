using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetContas;

public record ContaDto(
    Guid Id, string Nome, int Tipo, string? Instituicao, string? Pais, string Moeda,
    decimal Saldo, string? Identificador, Guid? EstruturaId, string? EstruturaNome,
    decimal ValorBRL, int QtdInvestimentos, bool AgregaInvestimentos,
    decimal? ValorPortfolio, decimal? LombardLimite, decimal? LombardUtilizado, decimal? LombardDisponivel, string? Status);

public record ContasResultDto(IReadOnlyList<ContaDto> Contas, decimal TotalBRL);

public record GetContasQuery : IRequest<ContasResultDto>;

public class GetContasQueryHandler(
    IContaFinanceiraRepository contaRepo,
    IInvestimentoRepository investimentoRepo,
    IEstruturaRepository estruturaRepo,
    IFxRateResolver fxResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetContasQuery, ContasResultDto>
{
    public async Task<ContasResultDto> Handle(GetContasQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;

        var contas        = await contaRepo.GetByUsuarioAsync(userId, ct);
        var investimentos = (await investimentoRepo.GetByUsuarioAsync(userId, ct)).ToList();
        var estruturas    = await estruturaRepo.GetByUsuarioAsync(userId, ct);
        var fx            = await fxResolver.GetRatesAsync(ct);

        var nomeEstrutura = estruturas.ToDictionary(e => e.Id, e => e.Nome);

        decimal ParaBRL(decimal v, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? v : v * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        var lista = new List<ContaDto>();
        foreach (var c in contas)
        {
            var agrega = c.Tipo == TipoContaFinanceira.InvestimentoCustodia;
            var ligados = agrega ? investimentos.Where(i => i.ContaId == c.Id).ToList() : [];

            // Conta de investimento/custódia deriva o valor dos investimentos ligados (em BRL).
            // Demais contas usam o saldo manual convertido para BRL.
            var valorBRL = agrega
                ? ligados.Sum(i => ParaBRL(i.ValorAtual, i.Moeda))
                : ParaBRL(c.Saldo, c.Moeda);

            var lombardDisp = c.LombardLimite.HasValue
                ? Math.Max(0, c.LombardLimite.Value - (c.LombardUtilizado ?? 0))
                : (decimal?)null;

            lista.Add(new ContaDto(
                c.Id, c.Nome, (int)c.Tipo, c.Instituicao, c.Pais, c.Moeda.ToString(),
                Math.Round(c.Saldo, 2), c.Identificador, c.EstruturaId,
                c.EstruturaId.HasValue && nomeEstrutura.TryGetValue(c.EstruturaId.Value, out var ne) ? ne : null,
                Math.Round(valorBRL, 2), ligados.Count, agrega,
                c.ValorPortfolio, c.LombardLimite, c.LombardUtilizado, lombardDisp, c.Status));
        }

        return new ContasResultDto(
            lista.OrderByDescending(c => c.ValorBRL).ToList(),
            Math.Round(lista.Sum(c => c.ValorBRL), 2));
    }
}
