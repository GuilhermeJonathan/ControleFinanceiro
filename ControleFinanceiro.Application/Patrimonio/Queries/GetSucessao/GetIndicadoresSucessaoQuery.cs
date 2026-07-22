using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;

/// <summary>
/// Scores efetivos (override do assessor OU cálculo automático) + os componentes,
/// para a UI mostrar o valor e saber se é manual.
/// </summary>
public record IndicadoresSucessaoDto(
    int GovernancaScore, int ConformidadeScore,
    int GovernancaCalculado, int ConformidadeCalculado,
    int? GovernancaOverride, int? ConformidadeOverride)
{
    public IndicadoresSucessaoDto() : this(0, 0, 0, 0, null, null) { }
}

public record GetIndicadoresSucessaoQuery : IRequest<IndicadoresSucessaoDto>;

public class GetIndicadoresSucessaoQueryHandler(
    IIndicadoresSucessaoRepository repo, IMediator mediator, ICurrentUser currentUser)
    : IRequestHandler<GetIndicadoresSucessaoQuery, IndicadoresSucessaoDto>
{
    public async Task<IndicadoresSucessaoDto> Handle(GetIndicadoresSucessaoQuery request, CancellationToken ct)
    {
        var grafo    = await mediator.Send(new GetEstruturasQuery(), ct);
        var sucessao = await mediator.Send(new GetSucessaoQuery(), ct);
        var contas   = await mediator.Send(new GetContasQuery(), ct);
        var planos   = (await mediator.Send(new GetPlanosAcaoQuery(), ct)).ToList();

        var (govCalc, confCalc) = IndicadoresSucessaoCalc.Calcular(grafo, sucessao, contas, planos);

        var ind = await repo.GetByUsuarioAsync(currentUser.UserId, ct);
        var govOv = ind?.GovernancaScore;
        var confOv = ind?.ConformidadeScore;

        return new IndicadoresSucessaoDto(
            govOv ?? govCalc, confOv ?? confCalc,
            govCalc, confCalc, govOv, confOv);
    }
}

// ── Save (upsert) ──────────────────────────────────────────────────────────

public record SaveIndicadoresSucessaoCommand(int? Governanca, int? Conformidade) : IRequest;

public class SaveIndicadoresSucessaoCommandHandler(
    IIndicadoresSucessaoRepository repo, ICurrentUser currentUser, IUnitOfWork uow)
    : IRequestHandler<SaveIndicadoresSucessaoCommand>
{
    public async Task Handle(SaveIndicadoresSucessaoCommand request, CancellationToken ct)
    {
        var existente = await repo.GetByUsuarioAsync(currentUser.UserId, ct);
        if (existente is null)
            await repo.AddAsync(new IndicadoresSucessao(currentUser.UserId, request.Governanca, request.Conformidade), ct);
        else
            existente.Atualizar(request.Governanca, request.Conformidade);
        await uow.SaveChangesAsync(ct);
    }
}
