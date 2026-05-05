using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosBusca;

public class GetLancamentosBuscaQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetLancamentosBuscaQuery, BuscaResultDto>
{
    public async Task<BuscaResultDto> Handle(GetLancamentosBuscaQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var (itens, total) = await repository.SearchAsync(
            request.Q.Trim(), page, pageSize, currentUser.UserId, cancellationToken);

        var dtos = itens.Select(l => new LancamentoBuscaItemDto(
            l.Id,
            l.Descricao,
            l.Data,
            l.Valor,
            l.Tipo,
            l.Situacao,
            l.Mes,
            l.Ano,
            l.CategoriaId,
            l.Categoria?.Nome,
            l.Categoria?.Icone,
            l.Categoria?.Cor,
            l.CartaoId,
            l.Cartao?.Nome,
            l.ParcelaAtual,
            l.TotalParcelas,
            l.IsRecorrente,
            l.GrupoParcelas,
            l.CriadoPorId,
            l.CriadoPorNome
        ));

        return new BuscaResultDto(total, dtos);
    }
}
