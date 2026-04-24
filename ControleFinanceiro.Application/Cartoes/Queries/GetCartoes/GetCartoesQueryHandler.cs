using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;

public class GetCartoesQueryHandler(ICartaoCreditoRepository cartaoRepository, ILancamentoRepository lancamentoRepository)
    : IRequestHandler<GetCartoesQuery, IEnumerable<CartaoDto>>
{
    public async Task<IEnumerable<CartaoDto>> Handle(GetCartoesQuery request, CancellationToken cancellationToken)
    {
        var cartoes = await cartaoRepository.GetAllWithParcelasAsync(cancellationToken);
        var result = new List<CartaoDto>();

        foreach (var cartao in cartoes)
        {
            var lancamentos = await lancamentoRepository.GetByCartaoMesAnoAsync(
                cartao.Id, request.Mes, request.Ano, cancellationToken);

            var dtos = lancamentos.Select(l => new CartaoLancamentoDto(
                l.Id, l.Descricao, l.Valor, l.Data, l.Situacao,
                l.ParcelaAtual, l.TotalParcelas));

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
