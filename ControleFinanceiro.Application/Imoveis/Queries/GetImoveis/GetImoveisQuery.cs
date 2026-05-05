using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Queries.GetImoveis;

public record GetImoveisQuery : IRequest<IEnumerable<ImovelDto>>;

public class GetImoveisQueryHandler(
    IImovelRepository repo,
    ICurrentUser currentUser) : IRequestHandler<GetImoveisQuery, IEnumerable<ImovelDto>>
{
    public async Task<IEnumerable<ImovelDto>> Handle(GetImoveisQuery request, CancellationToken ct)
    {
        var imoveis = await repo.GetAllAsync(currentUser.UserId, currentUser.PodeVerImoveis, ct);
        return imoveis.Select(i => new ImovelDto(
            i.Id,
            i.Descricao,
            i.Valor,
            i.Pros,
            i.Contras,
            i.Nota,
            i.DataVisita,
            i.NomeCorretor,
            i.TelefoneCorretor,
            i.Imobiliaria,
            i.Tipo,
            i.Fotos.OrderBy(f => f.Ordem).Select(f => new ImovelFotoDto(f.Id, f.Dados, f.Ordem)).ToList(),
            i.Comentarios.OrderByDescending(c => c.CriadoEm).Select(c => new ImovelComentarioDto(c.Id, c.Texto, c.CriadoEm)).ToList()
        ));
    }
}
