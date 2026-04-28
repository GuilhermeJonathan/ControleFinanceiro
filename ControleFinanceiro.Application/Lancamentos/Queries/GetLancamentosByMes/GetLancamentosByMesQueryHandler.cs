using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;

public class GetLancamentosByMesQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetLancamentosByMesQuery, IEnumerable<LancamentoDto>>
{
    public async Task<IEnumerable<LancamentoDto>> Handle(GetLancamentosByMesQuery request, CancellationToken cancellationToken)
    {
        var lancamentos = await repository.GetByMesAnoAsync(request.Mes, request.Ano, currentUser.UserId, cancellationToken);

        return lancamentos.Select(l => new LancamentoDto(
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
            l.DataPagamento));
    }
}
