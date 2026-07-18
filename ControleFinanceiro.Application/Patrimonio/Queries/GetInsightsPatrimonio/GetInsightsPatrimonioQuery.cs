using ControleFinanceiro.Application.Patrimonio.Queries.GetRebalanceamento;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetInsightsPatrimonio;

public record InsightDto(
    string Severidade,            // "alerta" | "atencao" | "positivo"
    string Titulo,
    string Mensagem,
    string RecomendacaoSugerida); // texto pronto pro assessor enviar como recomendação

/// <summary>
/// Insights determinísticos (sem IA) sobre o patrimônio do usuário efetivo:
/// concentração, alavancagem, fluxo, rentabilidade e desvio de alocação-alvo.
/// Cada insight já traz um texto sugerido para virar recomendação.
/// </summary>
public record GetInsightsPatrimonioQuery : IRequest<IEnumerable<InsightDto>>;

public class GetInsightsPatrimonioQueryHandler(ISender mediator)
    : IRequestHandler<GetInsightsPatrimonioQuery, IEnumerable<InsightDto>>
{
    private static readonly Dictionary<int, string> ClasseInvest = new()
    {
        [1] = "Ações", [2] = "FII", [3] = "ETF", [4] = "Renda Fixa",
        [5] = "Multimercado", [6] = "Cripto", [7] = "Exterior", [99] = "Outro",
    };

    public async Task<IEnumerable<InsightDto>> Handle(GetInsightsPatrimonioQuery request, CancellationToken cancellationToken)
    {
        var patr   = await mediator.Send(new GetResumoPatrimonialQuery(), cancellationToken);
        var invest = await mediator.Send(new GetResumoInvestimentosQuery(), cancellationToken);
        var rebal  = await mediator.Send(new GetRebalanceamentoQuery(), cancellationToken);

        var insights = new List<InsightDto>();

        // 1. Concentração do patrimônio numa categoria
        var topCat = patr.Composicao.OrderByDescending(c => c.Pct).FirstOrDefault();
        if (topCat is not null && topCat.Pct >= 60m)
            insights.Add(new InsightDto("atencao",
                "Patrimônio concentrado",
                $"{topCat.Pct:F0}% do seu patrimônio está em {topCat.Categoria}.",
                $"Notei que {topCat.Pct:F0}% do seu patrimônio está concentrado em {topCat.Categoria}. " +
                "Vale conversarmos sobre diversificar para reduzir o risco."));

        // 2. Alavancagem alta
        if (patr.AlavancagemPct >= 50m)
            insights.Add(new InsightDto("alerta",
                "Alavancagem alta",
                $"Suas dívidas representam {patr.AlavancagemPct:F0}% dos seus bens.",
                $"Suas dívidas hoje somam {patr.AlavancagemPct:F0}% dos seus bens. " +
                "Sugiro priorizarmos a redução do endividamento antes de novos aportes."));
        else if (patr.AlavancagemPct >= 30m)
            insights.Add(new InsightDto("atencao",
                "Endividamento moderado",
                $"Dívidas em {patr.AlavancagemPct:F0}% dos bens — de olho.",
                $"Seu endividamento está em {patr.AlavancagemPct:F0}% dos bens. Ainda saudável, mas vale monitorar."));

        // 3. Bens consomem mais do que geram
        if (patr.SaldoLiquidoMensalBRL < 0)
            insights.Add(new InsightDto("atencao",
                "Fluxo dos bens negativo",
                "Seus bens geram menos receita do que despesa por mês.",
                "Seus bens estão com fluxo de caixa negativo (mais despesa do que receita). " +
                "Vamos revisar custos ou a rentabilidade desses ativos?"));

        // 4. Rentabilidade dos investimentos negativa
        if (invest.RentabilidadePct is < 0)
            insights.Add(new InsightDto("alerta",
                "Investimentos no negativo",
                $"Sua carteira de investimentos está em {invest.RentabilidadePct:F1}%.",
                $"Sua carteira de investimentos acumula {invest.RentabilidadePct:F1}%. " +
                "Sugiro revisarmos a estratégia dos ativos com pior desempenho."));

        // 5. Concentração dos investimentos numa classe
        if (invest.TotalAtualBRL > 0)
        {
            var porClasse = invest.Investimentos
                .GroupBy(i => i.Tipo)
                .Select(g => new { g.Key, Total = g.Sum(i => i.ValorAtualBRL) })
                .OrderByDescending(g => g.Total).FirstOrDefault();
            if (porClasse is not null)
            {
                var pct = porClasse.Total / invest.TotalAtualBRL * 100m;
                if (pct >= 60m)
                    insights.Add(new InsightDto("atencao",
                        "Investimentos concentrados",
                        $"{pct:F0}% dos investimentos em {ClasseInvest.GetValueOrDefault(porClasse.Key, "uma classe")}.",
                        $"{pct:F0}% dos seus investimentos estão em {ClasseInvest.GetValueOrDefault(porClasse.Key, "uma única classe")}. " +
                        "Vale diversificar entre classes para equilibrar risco e retorno."));
            }
        }

        // 6. Desvio relevante da alocação-alvo
        if (rebal.TemAlvo)
        {
            var desvio = rebal.Classes.Where(c => Math.Abs(c.DesvioPct) >= 10m)
                .OrderByDescending(c => Math.Abs(c.DesvioPct)).FirstOrDefault();
            if (desvio is not null)
            {
                var nome = ClasseInvest.GetValueOrDefault(desvio.Tipo, "uma classe");
                var acima = desvio.DesvioPct > 0;
                insights.Add(new InsightDto("atencao",
                    "Fora da alocação-alvo",
                    $"{nome} está {Math.Abs(desvio.DesvioPct):F0}% {(acima ? "acima" : "abaixo")} do alvo.",
                    $"Sua posição em {nome} está {Math.Abs(desvio.DesvioPct):F0}% {(acima ? "acima" : "abaixo")} da meta definida. " +
                    $"Sugiro {(acima ? "reduzir" : "reforçar")} essa classe para voltar ao alvo."));
            }
        }

        if (insights.Count == 0)
            insights.Add(new InsightDto("positivo",
                "Tudo equilibrado",
                "Nenhum ponto de atenção relevante no momento.",
                "Sua carteira está equilibrada e sem pontos críticos no momento. Parabéns pela disciplina!"));

        return insights;
    }
}
