using ControleFinanceiro.Application.Common;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;

public class GetLancamentosByMesQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetLancamentosByMesQuery, PagedResult<LancamentoDto>>
{
    public async Task<PagedResult<LancamentoDto>> Handle(GetLancamentosByMesQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);

        var (lancamentos, total) = await repository.GetPagedByMesAnoAsync(
            request.Mes, request.Ano, currentUser.UserId, page, pageSize, cancellationToken);

        var dtos = lancamentos.Select(l => new LancamentoDto(
            l.Id, l.Descricao, l.Data, l.Valor, l.Tipo, l.Situacao,
            l.Mes, l.Ano, l.CategoriaId, l.Categoria?.Nome,
            l.CartaoId, l.Cartao?.Nome, l.ParcelaAtual, l.TotalParcelas, l.GrupoParcelas,
            l.Cartao?.DiaVencimento,
            l.ReceitaRecorrenteId,
            l.ReceitaRecorrente?.Tipo,
            l.ReceitaRecorrente?.ValorHora,
            l.ReceitaRecorrente?.QuantidadeHoras,
            l.IsRecorrente,
            l.ContaBancariaId,
            l.ContaBancaria?.Banco,
            l.DataPagamento,
            l.CriadoPorId,
            l.CriadoPorNome)).ToList();

        return new PagedResult<LancamentoDto>(dtos, total, page, pageSize);
    }
}
