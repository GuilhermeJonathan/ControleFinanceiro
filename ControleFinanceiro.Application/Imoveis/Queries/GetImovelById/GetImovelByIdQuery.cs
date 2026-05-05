using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Queries.GetImoveis;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Queries.GetImovelById;

public record GetImovelByIdQuery(Guid Id) : IRequest<ImovelDto>;

public class GetImovelByIdQueryHandler(
    IImovelRepository repo,
    ICurrentUser currentUser) : IRequestHandler<GetImovelByIdQuery, ImovelDto>
{
    public async Task<ImovelDto> Handle(GetImovelByIdQuery request, CancellationToken ct)
    {
        var imovel = await repo.GetByIdAsync(request.Id, currentUser.UserId, currentUser.PodeVerImoveis, ct)
            ?? throw new KeyNotFoundException("Imóvel não encontrado.");

        return new ImovelDto(
            imovel.Id,
            imovel.Descricao,
            imovel.Valor,
            imovel.Pros,
            imovel.Contras,
            imovel.Nota,
            imovel.DataVisita,
            imovel.NomeCorretor,
            imovel.TelefoneCorretor,
            imovel.Imobiliaria,
            imovel.Tipo,
            imovel.Fotos.OrderBy(f => f.Ordem).Select(f => new ImovelFotoDto(f.Id, f.Dados, f.Ordem)).ToList(),
            imovel.Comentarios.OrderByDescending(c => c.CriadoEm).Select(c => new ImovelComentarioDto(c.Id, c.Texto, c.CriadoEm)).ToList()
        );
    }
}
