using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Queries.GetOrcamento;

public class GetOrcamentoQueryHandler(
    ICategoriaRepository categoriaRepository,
    ILancamentoRepository lancamentoRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetOrcamentoQuery, IEnumerable<OrcamentoItemDto>>
{
    public async Task<IEnumerable<OrcamentoItemDto>> Handle(GetOrcamentoQuery request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;

        var categorias  = (await categoriaRepository.GetAllAsync(usuarioId, cancellationToken)).ToList();
        var lancamentos = (await lancamentoRepository.GetByMesAnoAsync(request.Mes, request.Ano, usuarioId, cancellationToken)).ToList();

        // Gasto por categoria (só débitos/pix, ignora lançamentos sem categoria)
        var gastos = lancamentos
            .Where(l => (l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix)
                        && l.CategoriaId != null)
            .GroupBy(l => l.CategoriaId!)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Valor));

        // Retorna categorias com limite definido primeiro, depois as sem limite que tiveram gasto
        var itens = categorias
            .Select(c => new OrcamentoItemDto(
                c.Id,
                c.Nome,
                c.LimiteMensal,
                gastos.GetValueOrDefault(c.Id, 0)
            ))
            .Where(i => i.LimiteMensal.HasValue || i.GastoAtual > 0)
            .OrderByDescending(i => i.LimiteMensal.HasValue)
            .ThenByDescending(i => i.LimiteMensal.HasValue
                ? i.GastoAtual / i.LimiteMensal!.Value
                : i.GastoAtual);

        return itens;
    }
}
