using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;

public class GetDashboardQueryHandler(ILancamentoRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        // Mês atual
        var lancamentos = (await repository.GetByMesAnoAsync(
            request.Mes, request.Ano, currentUser.UserId, cancellationToken)).ToList();

        var creditos = lancamentos.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
        var debitos  = lancamentos.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);
        var saldo    = creditos - debitos;

        var resumo = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix)
            .GroupBy(l => l.Categoria?.Nome ?? "Sem Categoria")
            .Select(g => new ResumoCategoriaDto(g.Key, g.Sum(l => l.Valor)))
            .OrderByDescending(r => r.Total);

        // Mês anterior para calcular variação
        var mesAnt = request.Mes == 1 ? 12 : request.Mes - 1;
        var anoAnt = request.Mes == 1 ? request.Ano - 1 : request.Ano;

        var anteriores = (await repository.GetByMesAnoAsync(
            mesAnt, anoAnt, currentUser.UserId, cancellationToken)).ToList();

        var credAnt  = anteriores.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
        var debAnt   = anteriores.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);
        var saldoAnt = credAnt - debAnt;

        return new DashboardDto(
            request.Mes, request.Ano,
            creditos, debitos, saldo,
            resumo,
            Variacacao(creditos,  credAnt),
            Variacacao(debitos,   debAnt),
            Variacacao(saldo,     saldoAnt)
        );
    }

    // Retorna null se não há base de comparação (mês anterior zerado)
    private static decimal? Variacacao(decimal atual, decimal anterior)
    {
        if (anterior == 0) return null;
        return Math.Round((atual - anterior) / Math.Abs(anterior) * 100, 1);
    }
}
