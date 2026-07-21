using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetDicasPatrimonio;

public class GetDicasPatrimonioQueryHandler(
    IAtivoPatrimonialRepository ativoRepo,
    IPassivoPatrimonialRepository passivoRepo,
    IFxRateResolver fxResolver,
    ICurrentUser currentUser,
    IAiService ai,
    ILogger<GetDicasPatrimonioQueryHandler> logger)
    : IRequestHandler<GetDicasPatrimonioQuery, IEnumerable<DicaFinanceiraDto>>
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<DicaFinanceiraDto>> Handle(
        GetDicasPatrimonioQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        var ativos   = (await ativoRepo.GetByUsuarioAsync(userId, cancellationToken)).ToList();
        var passivos = (await passivoRepo.GetByUsuarioAsync(userId, cancellationToken)).ToList();
        // Câmbio efetivo do tenant (globais não ocultas + custom do assessor).
        var taxas = await fxResolver.GetRatesAsync(cancellationToken);
        decimal ToBRL(decimal valor, string moeda) =>
            taxas.TryGetValue(moeda, out var t) ? valor * t : valor;

        decimal totalBens    = ativos.Sum(a => ToBRL(a.ValorAtual, a.Moeda.ToString()));
        decimal totalDividas = passivos.Sum(p => ToBRL(p.Valor, p.Moeda.ToString()));
        decimal patrimonioLiquido = totalBens - totalDividas;
        decimal alavancagem = totalBens > 0 ? Math.Round(totalDividas / totalBens * 100, 1) : 0;

        decimal receitaMensal  = ativos.Sum(a => ToBRL(a.ReceitaMensal, a.Moeda.ToString()));
        decimal despesaMensal  = ativos.Sum(a => ToBRL(a.DespesaMensal, a.Moeda.ToString()));
        decimal fluxoLiquido   = receitaMensal - despesaMensal;

        var topCategoria = ativos
            .GroupBy(a => a.Tipo.ToString())
            .Select(g => new { Nome = g.Key, Total = g.Sum(a => ToBRL(a.ValorAtual, a.Moeda.ToString())) })
            .OrderByDescending(g => g.Total)
            .FirstOrDefault();

        decimal? roiMedio = ativos.Any(a => a.ValorizacaoAnualPct.HasValue)
            ? Math.Round(ativos.Where(a => a.ValorizacaoAnualPct.HasValue)
                .Average(a => a.ValorizacaoAnualPct!.Value), 1)
            : null;

        try
        {
            return await GerarComIaAsync(
                totalBens, totalDividas, patrimonioLiquido, alavancagem,
                receitaMensal, despesaMensal, fluxoLiquido,
                roiMedio, topCategoria?.Nome, topCategoria?.Total,
                ativos.Count, passivos.Count,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "IA falhou para dicas de patrimônio. Usando fallback estático.");
            return GerarEstatico(
                totalBens, totalDividas, patrimonioLiquido, alavancagem,
                receitaMensal, despesaMensal, fluxoLiquido,
                roiMedio, topCategoria?.Nome);
        }
    }

    private async Task<IEnumerable<DicaFinanceiraDto>> GerarComIaAsync(
        decimal totalBens, decimal totalDividas, decimal patrimonioLiquido, decimal alavancagem,
        decimal receitaMensal, decimal despesaMensal, decimal fluxoLiquido,
        decimal? roiMedio, string? topCategoria, decimal? topCategoriaTotal,
        int qtdAtivos, int qtdPassivos,
        CancellationToken ct)
    {
        const string systemPrompt = """
            Você é um consultor de patrimônio especializado em alta renda no Brasil.
            Analise o balanço patrimonial do usuário e retorne APENAS um array JSON válido (sem markdown, sem texto extra)
            com até 3 recomendações priorizadas por impacto.

            Formato obrigatório de cada objeto:
            {"tipo":"critico|atencao|positivo","titulo":"texto curto (máx 40 chars)","descricao":"análise personalizada com valores reais (máx 150 chars)","dicaEducativa":"dica ou conceito de educação patrimonial relacionado (máx 130 chars)","acaoLabel":"texto ou null","acaoRota":"patrimonio|investimentos ou null"}

            Diretrizes:
            - Use os valores reais na descricao (patrimônio líquido, alavancagem, ROI, fluxo)
            - "dicaEducativa": ensine algo relevante — alocação, diversificação, alavancagem saudável, ROI benchmark
              Exemplos: "Alavancagem saudável: dívidas abaixo de 30% dos bens", "Imóveis geram ROI médio de 6-8% a.a. no Brasil"
              "Diversificação: nenhuma categoria deve superar 60% do patrimônio", "Fluxo positivo gera riqueza passiva composta"
            - "critico" → problema urgente, "atencao" → oportunidade de melhoria, "positivo" → como crescer o patrimônio
            - acaoRota só pode ser "patrimonio", "investimentos" ou null
            - Responda APENAS com o JSON array
            """;

        var userMessage = $"""
            Balanço patrimonial atual:
            - Total em bens:          {Brl(totalBens)}
            - Total em dívidas:       {Brl(totalDividas)}
            - Patrimônio líquido:     {Brl(patrimonioLiquido)}
            - Alavancagem:            {alavancagem}% dos bens
            - Receita mensal dos bens: {Brl(receitaMensal)}
            - Despesa mensal dos bens: {Brl(despesaMensal)}
            - Fluxo líquido mensal:   {Brl(fluxoLiquido)} ({(fluxoLiquido >= 0 ? "positivo" : "negativo")})
            - ROI médio anual:        {(roiMedio.HasValue ? $"{roiMedio:F1}% a.a." : "sem dados")}
            - Maior categoria:        {topCategoria ?? "sem dados"}{(topCategoriaTotal.HasValue ? $" — {Brl(topCategoriaTotal.Value)}" : "")}
            - Quantidade de bens:     {qtdAtivos}
            - Quantidade de dívidas:  {qtdPassivos}

            Gere até 3 recomendações: o que corrigir, o que otimizar e como fazer o patrimônio crescer mais.
            """;

        var raw  = await ai.ChatAsync(systemPrompt, userMessage, maxTokens: 600, temperature: 0.4f, ct);
        var json = ExtrairJson(raw);

        var dicas = JsonSerializer.Deserialize<List<DicaFinanceiraDto>>(json, _jsonOpts)
            ?? throw new InvalidOperationException("IA retornou JSON vazio.");

        if (dicas.Count == 0)
            throw new InvalidOperationException("IA não retornou nenhuma dica.");

        return dicas.Take(3);
    }

    private static IEnumerable<DicaFinanceiraDto> GerarEstatico(
        decimal totalBens, decimal totalDividas, decimal patrimonioLiquido, decimal alavancagem,
        decimal receitaMensal, decimal despesaMensal, decimal fluxoLiquido,
        decimal? roiMedio, string? topCategoria)
    {
        var dicas = new List<DicaFinanceiraDto>();

        if (alavancagem > 50)
            dicas.Add(new("critico",
                "Alavancagem Elevada",
                $"Dívidas representam {alavancagem:F0}% dos bens. Risco patrimonial alto — priorize quitação.",
                "Alavancagem saudável: dívidas abaixo de 30% do total de bens.",
                "Ver patrimônio", "patrimonio"));

        else if (alavancagem > 30)
            dicas.Add(new("atencao",
                "Alavancagem Moderada",
                $"Dívidas em {alavancagem:F0}% dos bens. Considere acelerar a quitação das dívidas caras.",
                "Dívidas acima de 30% aumentam vulnerabilidade a crises. Foque em quitação antes de novos investimentos.",
                "Ver patrimônio", "patrimonio"));

        if (fluxoLiquido < 0)
            dicas.Add(new("critico",
                "Fluxo Patrimonial Negativo",
                $"Seus bens custam {Brl(Math.Abs(fluxoLiquido))}/mês a mais do que rendem. Revise despesas dos ativos.",
                "Bens produtivos devem gerar fluxo positivo. Ativos com despesas altas reduzem o patrimônio líquido.",
                null, null));

        else if (fluxoLiquido > 0 && receitaMensal > 0)
            dicas.Add(new("positivo",
                "Fluxo Patrimonial Ativo",
                $"Seus bens geram {Brl(fluxoLiquido)}/mês de fluxo líquido. Continue diversificando ativos produtivos.",
                "Renda passiva recorrente é a base da liberdade financeira. Reinvista para efeito composto.",
                "Ver investimentos", "investimentos"));

        if (roiMedio.HasValue && roiMedio < 5)
            dicas.Add(new("atencao",
                "ROI Abaixo do Mercado",
                $"ROI médio de {roiMedio:F1}% a.a. está abaixo da inflação. Avalie reposicionamento de ativos.",
                "Benchmark: Tesouro Selic ~10,5% a.a. Ativos físicos devem superar ao menos a inflação (IPCA ~4-5%).",
                "Ver investimentos", "investimentos"));

        if (totalBens > 0 && totalDividas == 0 && fluxoLiquido == 0)
            dicas.Add(new("positivo",
                "Patrimônio Livre de Dívidas",
                $"Patrimônio líquido de {Brl(patrimonioLiquido)} sem alavancagem. Hora de fazer o patrimônio trabalhar.",
                "Ativos sem renda passiva são capital parado. Considere imóveis para renda ou fundos imobiliários (FIIs).",
                "Ver investimentos", "investimentos"));

        return dicas.Take(3);
    }

    private static string ExtrairJson(string raw)
    {
        var s = raw.Trim();
        if (s.StartsWith("```"))
        {
            var inicio = s.IndexOf('[');
            var fim    = s.LastIndexOf(']');
            if (inicio >= 0 && fim > inicio)
                return s[inicio..(fim + 1)];
        }
        return s;
    }

    private static string Brl(decimal valor) =>
        valor.ToString("C", new CultureInfo("pt-BR"));
}
