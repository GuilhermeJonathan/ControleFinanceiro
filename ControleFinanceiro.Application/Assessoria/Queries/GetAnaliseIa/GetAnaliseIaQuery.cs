using ControleFinanceiro.Application.Assessoria.Queries.GetSaudeFinanceira;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;
using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetAnaliseIa;

/// <summary>Rascunho gerado pela IA + o tipo de recomendação sugerido (Dica/Alerta).</summary>
public record AnaliseIaDto(string Rascunho, int TipoSugerido);

/// <summary>
/// Gera um rascunho de análise financeira em texto via IA (gpt-4o-mini) a partir
/// do score de saúde e do dashboard do usuário efetivo. Sob o modo view-as,
/// analisa o cliente visualizado. O assessor EDITA o rascunho antes de enviar
/// como recomendação — a IA nunca envia nada diretamente ao cliente.
/// </summary>
public record GetAnaliseIaQuery(int Mes, int Ano) : IRequest<AnaliseIaDto>;

public class GetAnaliseIaQueryHandler(ISender mediator, IAiService aiService)
    : IRequestHandler<GetAnaliseIaQuery, AnaliseIaDto>
{
    private const string SystemPrompt = """
        Você é um assessor financeiro brasileiro experiente escrevendo para um cliente.
        Recebe um resumo estruturado das finanças do mês e escreve uma análise curta
        (no máximo 4 parágrafos), em tom respeitoso e construtivo, em português do Brasil:
        1º parágrafo: visão geral do mês; 2º: o ponto mais forte; 3º: o ponto que mais
        precisa de atenção, com sugestão prática; 4º: fechamento encorajador.
        Não invente números — use apenas os fornecidos. Não use markdown nem emojis.
        """;

    public async Task<AnaliseIaDto> Handle(GetAnaliseIaQuery request, CancellationToken cancellationToken)
    {
        var saude = await mediator.Send(new GetSaudeFinanceiraQuery(request.Mes, request.Ano), cancellationToken);
        var dashboard = await mediator.Send(new GetDashboardQuery(request.Mes, request.Ano), cancellationToken);

        var contexto = $"""
            Mês: {request.Mes}/{request.Ano}
            Receitas: R$ {dashboard.TotalCreditos:F2}
            Despesas: R$ {dashboard.TotalDebitos:F2}
            Saldo: R$ {dashboard.Saldo:F2}
            Score de saúde financeira: {saude.ScoreGeral}/100 ({saude.Classificacao})
            Pilares:
            {string.Join("\n", saude.Pilares.Select(p => $"- {p.Nome}: {p.Pontos}/{p.Maximo} — {p.Detalhe}"))}
            Maiores gastos por categoria:
            {string.Join("\n", dashboard.ResumoDebitos.Take(5).Select(c => $"- {c.Categoria}: R$ {c.Total:F2}"))}
            """;

        var rascunho = await aiService.ChatAsync(SystemPrompt, contexto, maxTokens: 600, cancellationToken: cancellationToken);
        return new AnaliseIaDto(rascunho, (int)SugerirTipo(saude.ScoreGeral, dashboard.TotalCreditos, dashboard.TotalDebitos, dashboard.Saldo));
    }

    /// <summary>
    /// Classifica a recomendação como Alerta quando a situação exige atenção
    /// (score baixo, saldo negativo ou renda muito comprometida); senão, Dica.
    /// </summary>
    public static TipoRecomendacao SugerirTipo(int scoreGeral, decimal receitas, decimal despesas, decimal saldo)
    {
        var comprometimento = receitas > 0 ? despesas / receitas : 0m;
        var critico = scoreGeral < 50 || saldo < 0 || comprometimento >= 0.80m;
        return critico ? TipoRecomendacao.Alerta : TipoRecomendacao.Dica;
    }
}
