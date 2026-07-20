using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Parametros.Queries;

public record ParamItemDto(int Id, string Nome, string? Icone, int Ordem, bool Ativo, bool IsSystem);

// ── Tipos de Ativo ────────────────────────────────────────────────────────

public record GetTiposAtivoQuery : IRequest<List<ParamItemDto>>;

public class GetTiposAtivoQueryHandler(ITipoAtivoParamRepository repo)
    : IRequestHandler<GetTiposAtivoQuery, List<ParamItemDto>>
{
    public async Task<List<ParamItemDto>> Handle(GetTiposAtivoQuery request, CancellationToken ct)
    {
        var list = await repo.GetAllAsync(ct);
        return list.Select(x => new ParamItemDto(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem)).ToList();
    }
}

// ── Tipos de Investimento ─────────────────────────────────────────────────

public record GetTiposInvestimentoQuery : IRequest<List<ParamItemDto>>;

public class GetTiposInvestimentoQueryHandler(ITipoInvestimentoParamRepository repo)
    : IRequestHandler<GetTiposInvestimentoQuery, List<ParamItemDto>>
{
    public async Task<List<ParamItemDto>> Handle(GetTiposInvestimentoQuery request, CancellationToken ct)
    {
        var list = await repo.GetAllAsync(ct);
        return list.Select(x => new ParamItemDto(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem)).ToList();
    }
}

// ── Moedas

public record MoedaParamDto(int Id, string Codigo, string Nome, decimal CotacaoBRL, int Ordem, bool Ativo, bool IsSystem, DateTime? CotacaoAtualizadaEm);
public record GetMoedasQuery : IRequest<List<MoedaParamDto>>;

public class GetMoedasQueryHandler(IMoedaParamRepository repo)
    : IRequestHandler<GetMoedasQuery, List<MoedaParamDto>>
{
    public async Task<List<MoedaParamDto>> Handle(GetMoedasQuery request, CancellationToken ct)
    {
        var list = await repo.GetAllAsync(ct);
        return list.Select(x => new MoedaParamDto(x.Id, x.Codigo, x.Nome, x.CotacaoBRL, x.Ordem, x.Ativo, x.IsSystem, x.CotacaoAtualizadaEm)).ToList();
    }
}
