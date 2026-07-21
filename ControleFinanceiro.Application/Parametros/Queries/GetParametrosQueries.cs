using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Parametros.Queries;

/// <param name="AssessorId">null = tipo global (catálogo do admin); preenchido = custom da assessoria.</param>
/// <param name="Oculto">Para a assessoria dona: o default global está ocultado do catálogo dela.</param>
/// <param name="PodeEditar">O usuário atual pode editar/excluir este item (admin+global ou assessor+seu custom).</param>
public record ParamItemDto(
    int Id, string Nome, string? Icone, int Ordem, bool Ativo, bool IsSystem,
    Guid? AssessorId, bool Oculto, bool PodeEditar);

// ── Tipos de Ativo ────────────────────────────────────────────────────────

public record GetTiposAtivoQuery : IRequest<List<ParamItemDto>>;

public class GetTiposAtivoQueryHandler(
    ITipoAtivoParamRepository repo,
    IParametroOcultoRepository ocultoRepo,
    IAssessoriaOwnerResolver ownerResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetTiposAtivoQuery, List<ParamItemDto>>
{
    public async Task<List<ParamItemDto>> Handle(GetTiposAtivoQuery request, CancellationToken ct)
    {
        if (currentUser.IsAdmin)
        {
            var globais = await repo.GetGlobaisAsync(ct);
            return globais.Select(x => Map(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem, x.AssessorId, false, true)).ToList();
        }

        var owner = await ownerResolver.ResolveOwnerAsync(ct);
        if (owner is null)
        {
            var globais = await repo.GetGlobaisAsync(ct);
            return globais.Select(x => Map(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem, x.AssessorId, false, false)).ToList();
        }

        var todos      = await repo.GetGlobaisEDoAssessorAsync(owner.Value, ct);
        var ocultosIds = await ocultoRepo.GetIdsOcultosAsync(owner.Value, TipoParametroCatalogo.TipoAtivo, ct);
        var ehDono     = currentUser.IsAssessor; // assessor não-admin: gerencia o próprio catálogo

        var res = new List<ParamItemDto>();
        foreach (var x in todos)
        {
            var isGlobal = x.AssessorId is null;
            var oculto   = isGlobal && ocultosIds.Contains(x.Id);
            if (!ehDono && oculto) continue; // consumo (cliente/corretor): esconde ocultos
            res.Add(Map(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem, x.AssessorId, oculto, ehDono && !isGlobal));
        }
        return res;
    }

    private static ParamItemDto Map(int id, string nome, string? icone, int ordem, bool ativo, bool isSystem, Guid? assessorId, bool oculto, bool podeEditar) =>
        new(id, nome, icone, ordem, ativo, isSystem, assessorId, oculto, podeEditar);
}

// ── Tipos de Investimento ─────────────────────────────────────────────────

public record GetTiposInvestimentoQuery : IRequest<List<ParamItemDto>>;

public class GetTiposInvestimentoQueryHandler(
    ITipoInvestimentoParamRepository repo,
    IParametroOcultoRepository ocultoRepo,
    IAssessoriaOwnerResolver ownerResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetTiposInvestimentoQuery, List<ParamItemDto>>
{
    public async Task<List<ParamItemDto>> Handle(GetTiposInvestimentoQuery request, CancellationToken ct)
    {
        if (currentUser.IsAdmin)
        {
            var globais = await repo.GetGlobaisAsync(ct);
            return globais.Select(x => new ParamItemDto(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem, x.AssessorId, false, true)).ToList();
        }

        var owner = await ownerResolver.ResolveOwnerAsync(ct);
        if (owner is null)
        {
            var globais = await repo.GetGlobaisAsync(ct);
            return globais.Select(x => new ParamItemDto(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem, x.AssessorId, false, false)).ToList();
        }

        var todos      = await repo.GetGlobaisEDoAssessorAsync(owner.Value, ct);
        var ocultosIds = await ocultoRepo.GetIdsOcultosAsync(owner.Value, TipoParametroCatalogo.TipoInvestimento, ct);
        var ehDono     = currentUser.IsAssessor;

        var res = new List<ParamItemDto>();
        foreach (var x in todos)
        {
            var isGlobal = x.AssessorId is null;
            var oculto   = isGlobal && ocultosIds.Contains(x.Id);
            if (!ehDono && oculto) continue;
            res.Add(new ParamItemDto(x.Id, x.Nome, x.Icone, x.Ordem, x.Ativo, x.IsSystem, x.AssessorId, oculto, ehDono && !isGlobal));
        }
        return res;
    }
}

// ── Moedas (global + override por assessoria) ──────────────────────────────

public record MoedaParamDto(
    int Id, string Codigo, string Nome, decimal CotacaoBRL, int Ordem, bool Ativo, bool IsSystem,
    DateTime? CotacaoAtualizadaEm, Guid? AssessorId, bool Oculto, bool PodeEditar);

public record GetMoedasQuery : IRequest<List<MoedaParamDto>>;

public class GetMoedasQueryHandler(
    IMoedaParamRepository repo,
    IParametroOcultoRepository ocultoRepo,
    IAssessoriaOwnerResolver ownerResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetMoedasQuery, List<MoedaParamDto>>
{
    private static MoedaParamDto Map(Domain.Entities.MoedaParam x, bool oculto, bool podeEditar) =>
        new(x.Id, x.Codigo, x.Nome, x.CotacaoBRL, x.Ordem, x.Ativo, x.IsSystem, x.CotacaoAtualizadaEm, x.AssessorId, oculto, podeEditar);

    public async Task<List<MoedaParamDto>> Handle(GetMoedasQuery request, CancellationToken ct)
    {
        if (currentUser.IsAdmin)
        {
            var globais = await repo.GetGlobaisAsync(ct);
            return globais.Select(x => Map(x, false, true)).ToList();
        }

        var owner = await ownerResolver.ResolveOwnerAsync(ct);
        if (owner is null)
        {
            var globais = await repo.GetGlobaisAsync(ct);
            return globais.Select(x => Map(x, false, false)).ToList();
        }

        var todos      = await repo.GetGlobaisEDoAssessorAsync(owner.Value, ct);
        var ocultosIds = await ocultoRepo.GetIdsOcultosAsync(owner.Value, TipoParametroCatalogo.Moeda, ct);
        var ehDono     = currentUser.IsAssessor;

        var res = new List<MoedaParamDto>();
        foreach (var x in todos)
        {
            var isGlobal = x.AssessorId is null;
            var oculto   = isGlobal && ocultosIds.Contains(x.Id);
            if (!ehDono && oculto) continue; // consumo (cliente/corretor): esconde ocultas
            res.Add(Map(x, oculto, ehDono && !isGlobal));
        }
        return res;
    }
}
