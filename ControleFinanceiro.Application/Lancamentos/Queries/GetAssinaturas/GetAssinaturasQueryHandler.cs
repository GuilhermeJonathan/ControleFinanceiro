using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetAssinaturas;

public class GetAssinaturasQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetAssinaturasQuery, List<AssinaturaDto>>
{
    public async Task<List<AssinaturaDto>> Handle(GetAssinaturasQuery request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;
        var todos = await repository.GetRecorrentesAsync(usuarioId, cancellationToken);

        return todos
            .GroupBy(l => l.ReceitaRecorrenteId.HasValue
                ? l.ReceitaRecorrenteId.Value.ToString()
                : l.Descricao)
            .Select(g =>
            {
                var ordered = g.OrderBy(l => l.Data).ToList();
                var primeiro = ordered.First();
                var proximo = ordered
                    .Where(l => l.Situacao is SituacaoLancamento.AVencer or SituacaoLancamento.Vencido)
                    .OrderBy(l => l.Data)
                    .FirstOrDefault();
                var pagos = ordered.Count(l => l.Situacao is SituacaoLancamento.Pago);

                return new AssinaturaDto(
                    GrupoId:           g.Key,
                    Descricao:         primeiro.Descricao,
                    ValorMensal:       primeiro.Valor,
                    CategoriaNome:     primeiro.Categoria?.Nome,
                    CategoriaIcone:   primeiro.Categoria?.Icone,
                    CategoriaCor:     primeiro.Categoria?.Cor,
                    ProximoVencimento: proximo?.Data,
                    TotalLancamentos:  ordered.Count,
                    LancamentosPagos:  pagos);
            })
            .OrderBy(a => a.ProximoVencimento ?? DateTime.MaxValue)
            .ToList();
    }
}
