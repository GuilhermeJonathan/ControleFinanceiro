using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetProjecao;

public class GetProjecaoQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetProjecaoQuery, IEnumerable<ProjecaoMesDto>>
{
    private static readonly string[] Meses =
        ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"];

    public async Task<IEnumerable<ProjecaoMesDto>> Handle(GetProjecaoQuery request, CancellationToken cancellationToken)
    {
        // Janela: 2 meses atrás até 9 meses à frente (12 meses total)
        var inicio = new DateOnly(request.AnoBase, request.MesBase, 1).AddMonths(-2);
        var fim    = new DateOnly(request.AnoBase, request.MesBase, 1).AddMonths(9);

        var lancamentos = await repository.GetProjecaoAsync(
            inicio.Month, inicio.Year,
            fim.Month, fim.Year,
            currentUser.UserId, cancellationToken);

        // Agrupa por Mes/Ano e agrega receitas/despesas
        var resultado = new List<ProjecaoMesDto>();
        for (int offset = -2; offset <= 9; offset++)
        {
            var d = new DateOnly(request.AnoBase, request.MesBase, 1).AddMonths(offset);
            var doMes    = lancamentos.Where(l => l.Mes == d.Month && l.Ano == d.Year).ToList();
            var creditos = doMes.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
            var debitos  = doMes.Where(l => l.Tipo != TipoLancamento.Credito).Sum(l => l.Valor);
            resultado.Add(new ProjecaoMesDto(d.Month, d.Year, Meses[d.Month - 1], creditos, debitos));
        }

        return resultado;
    }
}
