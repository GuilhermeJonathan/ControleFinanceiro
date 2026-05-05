using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Metas.Queries.GetMetas;

public record MetaDto(
    Guid Id,
    string Titulo,
    string? Descricao,
    decimal ValorMeta,
    decimal ValorAtual,
    DateTime? DataMeta,
    StatusMeta Status,
    string? Capa,
    string? CorFundo,
    DateTime CriadoEm,
    decimal? ContribuicaoMensalValor,
    int? ContribuicaoDia);

public record GetMetasQuery : IRequest<IEnumerable<MetaDto>>;

public class GetMetasQueryHandler(
    IMetaRepository repo,
    ICurrentUser currentUser) : IRequestHandler<GetMetasQuery, IEnumerable<MetaDto>>
{
    public async Task<IEnumerable<MetaDto>> Handle(GetMetasQuery request, CancellationToken ct)
    {
        var metas = await repo.GetAllAsync(currentUser.UserId, ct);
        return metas.Select(m => new MetaDto(
            m.Id, m.Titulo, m.Descricao, m.ValorMeta, m.ValorAtual,
            m.DataMeta, m.Status, m.Capa, m.CorFundo, m.CriadoEm,
            m.ContribuicaoMensalValor, m.ContribuicaoDia));
    }
}
