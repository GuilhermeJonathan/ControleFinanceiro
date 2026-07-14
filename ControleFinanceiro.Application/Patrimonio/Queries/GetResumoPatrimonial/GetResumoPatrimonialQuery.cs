using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;

public record AtivoResumoDto(
    Guid Id,
    string Nome,
    int Tipo,
    string Moeda,
    decimal ValorAtual,
    decimal? ValorizacaoAnualPct);

public record TotalPorMoedaDto(string Moeda, decimal Total, int Quantidade);

public record ResumoPatrimonialDto(
    int QtdAtivos,
    IEnumerable<TotalPorMoedaDto> TotaisPorMoeda,
    decimal TotalConsolidadoBRL,   // usa câmbio stub — ver FxStub
    bool CambioEstimado,           // true enquanto o câmbio for stub (não tempo real)
    IEnumerable<AtivoResumoDto> Ativos)
{
    public ResumoPatrimonialDto() 
        : this(0, [], 0m, true, [])
    {
    }
}

/// <summary>
/// Consolida o patrimônio do usuário efetivo (o próprio, ou o cliente sob view-as
/// do assessor). Multi-moeda com câmbio ESTUB (fixo) — a cotação em tempo real é
/// item de fase posterior (exige feed pago + licenciamento). Marcado por CambioEstimado.
/// </summary>
public record GetResumoPatrimonialQuery : IRequest<ResumoPatrimonialDto>;

public class GetResumoPatrimonialQueryHandler(
    IAtivoPatrimonialRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetResumoPatrimonialQuery, ResumoPatrimonialDto>
{
    // Câmbio provisório para consolidação em BRL. TODO: substituir por cotação real.
    private static readonly Dictionary<MoedaPatrimonio, decimal> FxStub = new()
    {
        [MoedaPatrimonio.BRL] = 1.00m,
        [MoedaPatrimonio.USD] = 5.40m,
        [MoedaPatrimonio.EUR] = 5.90m,
        [MoedaPatrimonio.CHF] = 6.10m,
        [MoedaPatrimonio.GBP] = 6.90m,
    };

    public async Task<ResumoPatrimonialDto> Handle(GetResumoPatrimonialQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var ativos = (await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();

            var totaisPorMoeda = ativos
                .GroupBy(a => a.Moeda)
                .Select(g => new TotalPorMoedaDto(g.Key.ToString(), g.Sum(a => a.ValorAtual), g.Count()))
                .OrderByDescending(t => t.Total)
                .ToList();

            var totalBRL = ativos.Sum(a => a.ValorAtual * FxStub.GetValueOrDefault(a.Moeda, 1m));

            var ativosDto = ativos.Select(a => new AtivoResumoDto(
                a.Id, a.Nome, (int)a.Tipo, a.Moeda.ToString(), a.ValorAtual, a.ValorizacaoAnualPct));

            return new ResumoPatrimonialDto(
                ativos.Count, totaisPorMoeda, Math.Round(totalBRL, 2), CambioEstimado: true, ativosDto);
        }
        catch (Exception ex) { 
            Console.WriteLine(ex);
        }

        return new ResumoPatrimonialDto();
    }
}
