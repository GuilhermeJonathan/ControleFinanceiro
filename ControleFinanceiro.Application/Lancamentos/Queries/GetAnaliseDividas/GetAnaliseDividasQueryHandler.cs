using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;
using ControleFinanceiro.Application.Lancamentos.Queries.GetParceladosVigentes;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetAnaliseDividas;

public class GetAnaliseDividasQueryHandler(
    ILancamentoRepository lancamentoRepository,
    ICurrentUser currentUser,
    IAiService ai,
    ILogger<GetAnaliseDividasQueryHandler> logger)
    : IRequestHandler<GetAnaliseDividasQuery, IEnumerable<DicaFinanceiraDto>>
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<DicaFinanceiraDto>> Handle(
        GetAnaliseDividasQuery request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;
        var lancamentos = (await lancamentoRepository
            .GetParceladosVigentesAsync(usuarioId, cancellationToken)).ToList();

        if (lancamentos.Count == 0)
            return [];

        // Re-agrupa igual ao GetParceladosVigentes
        var grupos = lancamentos
            .GroupBy(l => l.GrupoParcelas.HasValue
                ? l.GrupoParcelas.Value.ToString()
                : $"{l.Descricao}|{l.CartaoId}")
            .Select(g =>
            {
                var primeiro = g.OrderBy(l => l.ParcelaAtual).First();
                return new ParceladoVigenteItemDto(
                    Descricao:     primeiro.Descricao,
                    CategoriaNome: primeiro.Categoria?.Nome,
                    CartaoNome:    primeiro.Cartao?.Nome,
                    PrimeiraData:  g.Min(l => l.Data),
                    ParcelaMin:    g.Min(l => l.ParcelaAtual!.Value),
                    TotalParcelas: primeiro.TotalParcelas ?? g.Count(),
                    ValorParcela:  primeiro.Valor,
                    SaldoRestante: g.Sum(l => l.Valor)
                );
            })
            .OrderByDescending(i => i.SaldoRestante)
            .ToList();

        var totalDivida     = grupos.Sum(i => i.SaldoRestante);
        var mensalidade     = grupos.Sum(i => i.ValorParcela);
        var totalPago       = grupos.Sum(i => (i.ParcelaMin - 1) * i.ValorParcela);
        var totalGeral      = grupos.Sum(i => i.TotalParcelas   * i.ValorParcela);
        var pctQuitacao     = totalGeral > 0 ? Math.Round(totalPago / totalGeral * 100, 1) : 0m;

        // Maior dívida e data de quitação mais distante
        var maisGrande = grupos.OrderByDescending(i => i.SaldoRestante).First();
        var quitacaoFinal = grupos
            .Select(i => { var d = i.PrimeiraData.AddMonths(i.TotalParcelas - 1); return d; })
            .Max();

        var mesesRestantes = ((quitacaoFinal.Year - DateTime.Today.Year) * 12)
                           + (quitacaoFinal.Month - DateTime.Today.Month);

        // Top categoria
        var topCat = grupos
            .Where(i => i.CategoriaNome is not null)
            .GroupBy(i => i.CategoriaNome!)
            .Select(g => new { Nome = g.Key, Total = g.Sum(i => i.SaldoRestante) })
            .OrderByDescending(g => g.Total)
            .FirstOrDefault();

        try
        {
            return await GerarComIaAsync(
                grupos, totalDivida, mensalidade, pctQuitacao,
                mesesRestantes, quitacaoFinal, topCat?.Nome, topCat?.Total,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "IAiService falhou para análise de dívidas. Usando fallback estático.");
            return GerarEstatico(totalDivida, mensalidade, pctQuitacao, mesesRestantes, topCat?.Nome, grupos.Count);
        }
    }

    private async Task<IEnumerable<DicaFinanceiraDto>> GerarComIaAsync(
        List<ParceladoVigenteItemDto> grupos,
        decimal totalDivida, decimal mensalidade, decimal pctQuitacao,
        int mesesRestantes, DateTime quitacaoFinal,
        string? topCatNome, decimal? topCatTotal,
        CancellationToken ct)
    {
        const string systemPrompt = """
            Você é um analista financeiro especialista em gestão de dívidas e finanças pessoais no Brasil.
            Analise o perfil de dívidas parceladas do usuário e retorne APENAS um array JSON válido (sem markdown, sem texto extra)
            com exatamente 3 insights priorizados por impacto.

            Formato obrigatório de cada objeto:
            {"tipo":"critico|atencao|positivo","titulo":"texto curto (máx 40 chars)","descricao":"análise personalizada com valores reais (máx 160 chars)","dicaEducativa":"estratégia ou conceito financeiro aplicável (máx 130 chars)","acaoLabel":null,"acaoRota":null}

            Diretrizes para uma análise de dívidas:
            - Avalie o total da dívida, mensalidade e prazo de quitação
            - Identifique se a dívida é alta, moderada ou controlada em relação à mensalidade
            - Sugira estratégias: amortização antecipada, refinanciamento, priorização de quitar as de maior saldo
            - Mencione conceitos como: "bola de neve" (quitar menores primeiro), "avalanche" (quitar maiores juros primeiro)
            - Se o prazo for longo (>24 meses), destaque o impacto do custo do dinheiro no tempo
            - Cite produtos para substituir dívidas caras: CDC, consórcio, empréstimo consignado
            - "critico" → situação de alerta, "atencao" → oportunidade de melhoria, "positivo" → parabéns/como acelerar
            - Responda APENAS com o JSON array
            """;

        var sb = new StringBuilder();
        sb.AppendLine($"Perfil de dívidas parceladas ativas:");
        sb.AppendLine($"- Total em dívidas:        {Brl(totalDivida)}");
        sb.AppendLine($"- Mensalidade total:        {Brl(mensalidade)}/mês");
        sb.AppendLine($"- Progresso de quitação:    {pctQuitacao:F1}% já pago");
        sb.AppendLine($"- Prazo final de quitação:  {quitacaoFinal:MM/yyyy} ({mesesRestantes} meses restantes)");
        sb.AppendLine($"- Número de dívidas ativas: {grupos.Count}");
        if (topCatNome is not null)
            sb.AppendLine($"- Maior categoria:          {topCatNome} ({Brl(topCatTotal ?? 0)})");
        sb.AppendLine();
        sb.AppendLine("Detalhamento das 5 maiores:");
        foreach (var g in grupos.Take(5))
        {
            var end = g.PrimeiraData.AddMonths(g.TotalParcelas - 1);
            var restantes = g.TotalParcelas - g.ParcelaMin + 1;
            sb.AppendLine($"  • {g.Descricao}: {Brl(g.SaldoRestante)} restante | {g.ParcelaMin}/{g.TotalParcelas}x | {Brl(g.ValorParcela)}/mês | quita {end:MM/yyyy} ({restantes} parcelas)");
        }
        sb.AppendLine();
        sb.AppendLine("Gere 3 insights de analista financeiro sobre esse perfil de dívidas: o que é crítico, o que pode melhorar e como acelerar a quitação.");

        var raw  = await ai.ChatAsync(systemPrompt, sb.ToString(), maxTokens: 700, temperature: 0.4f, ct);
        var json = ExtrairJson(raw);

        var dicas = JsonSerializer.Deserialize<List<DicaFinanceiraDto>>(json, _jsonOpts)
            ?? throw new InvalidOperationException("IA retornou JSON vazio.");

        if (dicas.Count == 0)
            throw new InvalidOperationException("IA não retornou nenhuma dica.");

        logger.LogInformation("IAiService gerou {Count} insight(s) para análise de dívidas", dicas.Count);
        return dicas.Take(3);
    }

    private static IEnumerable<DicaFinanceiraDto> GerarEstatico(
        decimal totalDivida, decimal mensalidade, decimal pctQuitacao,
        int mesesRestantes, string? topCatNome, int qtdDividas)
    {
        var dicas = new List<DicaFinanceiraDto>();

        if (mesesRestantes > 36)
            dicas.Add(new("critico",
                "Prazo Muito Longo",
                $"Quitação em {mesesRestantes} meses. Dívidas longas custam muito mais — considere amortizar antecipadamente.",
                "Estratégia avalanche: pague o mínimo em todas e use o excedente na dívida de maior saldo/juro.",
                null, null));
        else if (totalDivida > mensalidade * 24)
            dicas.Add(new("atencao",
                "Volume Alto de Dívidas",
                $"{Brl(totalDivida)} em dívidas = {Math.Round(totalDivida / mensalidade, 0)} mensalidades. Priorize quitar as menores primeiro.",
                "Método bola de neve: quite a menor dívida primeiro, libera parcela e redirecione para a próxima.",
                null, null));
        else
            dicas.Add(new("positivo",
                "Dívidas Sob Controle",
                $"{pctQuitacao:F0}% já quitado. Mantenha o ritmo e considere antecipar parcelas quando possível.",
                "Antecipar parcelas sem juros reduz o saldo devedor e libera renda mensal mais cedo.",
                null, null));

        if (qtdDividas > 5)
            dicas.Add(new("atencao",
                "Muitas Dívidas Simultâneas",
                $"{qtdDividas} parcelamentos ativos. Consolide ou priorize para não perder o controle.",
                "Evite acumular mais de 3-4 parcelamentos simultâneos — a soma das parcelas compromete a renda.",
                null, null));
        else
            dicas.Add(new("positivo",
                "Amortização Antecipada",
                $"Com {pctQuitacao:F0}% pago, você tem poder de negociação. Veja se há desconto para quitar antecipado.",
                "Muitos contratos permitem amortização com desconto proporcional — negocie diretamente com a loja/banco.",
                null, null));

        dicas.Add(new("atencao",
            "Mensalidade x Renda",
            $"{Brl(mensalidade)}/mês comprometido com parcelamentos. Compare com sua renda para avaliar o impacto.",
            "Parcelas não devem passar de 30% da renda. Se passar, priorize quitações para recuperar folga.",
            null, null));

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
