using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;

public class GetDashboardQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        // Mês atual
        var lancamentos = (await repository.GetByMesAnoAsync(
            request.Mes, request.Ano, userId, cancellationToken)).ToList();

        var creditos = lancamentos.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
        var debitos  = lancamentos.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);
        var saldo    = creditos - debitos;

        var resumo = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix)
            .GroupBy(l => new { Nome = l.Categoria?.Nome ?? "Sem Categoria", l.Categoria?.Icone, l.Categoria?.Cor })
            .Select(g => new ResumoCategoriaDto(g.Key.Nome, g.Sum(l => l.Valor), g.Key.Icone, g.Key.Cor))
            .OrderByDescending(r => r.Total);

        // Mês anterior para calcular variação
        var mesAnt = request.Mes == 1 ? 12 : request.Mes - 1;
        var anoAnt = request.Mes == 1 ? request.Ano - 1 : request.Ano;

        var anteriores = (await repository.GetByMesAnoAsync(
            mesAnt, anoAnt, userId, cancellationToken)).ToList();

        var credAnt  = anteriores.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
        var debAnt   = anteriores.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);
        var saldoAnt = credAnt - debAnt;

        // ── Saúde financeira ─────────────────────────────────────────────────────

        // Comprometimento de renda: débitos ÷ créditos × 100
        decimal? comprometimento = creditos > 0
            ? Math.Round(debitos / creditos * 100, 1)
            : null;

        // Dias de reserva: saldo acumulado dos lançamentos ÷ gasto médio diário (últimos 3 meses)
        int? diasReserva = null;
        var saldoAcumulado = await repository.GetSaldoAcumuladoAsync(request.Mes, request.Ano, userId, cancellationToken);
        if (saldoAcumulado > 0)
        {
            // Coleta débitos dos últimos 3 meses (atual + 2 anteriores)
            var mes2 = mesAnt == 1 ? 12 : mesAnt - 1;
            var ano2 = mesAnt == 1 ? anoAnt - 1 : anoAnt;
            var doisMesesAtras = (await repository.GetByMesAnoAsync(mes2, ano2, userId, cancellationToken)).ToList();

            var totalDebitos3Meses =
                debitos +
                debAnt +
                doisMesesAtras.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);

            var gastoMedioDiario = totalDebitos3Meses / 90m;
            if (gastoMedioDiario > 0)
                diasReserva = (int)(saldoAcumulado / gastoMedioDiario);
        }

        return new DashboardDto(
            request.Mes, request.Ano,
            creditos, debitos, saldo,
            resumo,
            Variacacao(creditos,  credAnt),
            Variacacao(debitos,   debAnt),
            Variacacao(saldo,     saldoAnt),
            diasReserva,
            comprometimento
        );
    }

    // Retorna null se não há base de comparação (mês anterior zerado)
    private static decimal? Variacacao(decimal atual, decimal anterior)
    {
        if (anterior == 0) return null;
        return Math.Round((atual - anterior) / Math.Abs(anterior) * 100, 1);
    }
}
