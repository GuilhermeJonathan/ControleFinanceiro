using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;

public class GetCartoesQueryHandler(
    ICartaoCreditoRepository cartaoRepository,
    ILancamentoRepository lancamentoRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetCartoesQuery, IEnumerable<CartaoDto>>
{
    public async Task<IEnumerable<CartaoDto>> Handle(GetCartoesQuery request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;
        var cartoes = await cartaoRepository.GetAllWithParcelasAsync(usuarioId, cancellationToken);
        var result = new List<CartaoDto>();

        foreach (var cartao in cartoes)
        {
            var lancamentos = await lancamentoRepository.GetByCartaoMesAnoAsync(
                cartao.Id, request.Mes, request.Ano, usuarioId, cancellationToken);

            var dtos = lancamentos.Select(l => new CartaoLancamentoDto(
                l.Id, l.Descricao, l.Valor, l.Data, l.Situacao,
                l.ParcelaAtual, l.TotalParcelas, l.Categoria?.Nome));

            result.Add(new CartaoDto(
                cartao.Id,
                cartao.Nome,
                cartao.DiaVencimento,
                lancamentos.Sum(l => l.Valor),
                dtos));
        }

        return result;
    }
}
