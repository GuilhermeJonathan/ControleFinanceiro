using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Parametros.Queries;

public record SubtipoInvestimentoDto(int Id, int TipoInvestimentoId, string Nome, int Ordem, bool Ativo, bool IsSystem);

/// <summary>Subtipos de investimento. Se TipoInvestimentoId != null, filtra por aquele tipo.</summary>
public record GetSubtiposInvestimentoQuery(int? TipoInvestimentoId = null) : IRequest<List<SubtipoInvestimentoDto>>;

public class GetSubtiposInvestimentoQueryHandler(ISubtipoInvestimentoParamRepository repo)
    : IRequestHandler<GetSubtiposInvestimentoQuery, List<SubtipoInvestimentoDto>>
{
    public async Task<List<SubtipoInvestimentoDto>> Handle(GetSubtiposInvestimentoQuery request, CancellationToken ct)
    {
        var itens = request.TipoInvestimentoId is int tid
            ? await repo.GetByTipoAsync(tid, ct)
            : await repo.GetAllAsync(ct);
        return itens.Select(s => new SubtipoInvestimentoDto(s.Id, s.TipoInvestimentoId, s.Nome, s.Ordem, s.Ativo, s.IsSystem)).ToList();
    }
}
