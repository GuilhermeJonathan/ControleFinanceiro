using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;

public class GetDicasQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser,
    IAiService ai,
    ILogger<GetDicasQueryHandler> logger)
    : IRequestHandler<GetDicasQuery, IEnumerable<DicaFinanceiraDto>>
{
    private static readonly string[] Meses =
        ["Janeiro","Fevereiro","Março","Abril","Maio","Junho",
         "Julho","Agosto","Setembro","Outubro","Novembro","Dezembro"];

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<DicaFinanceiraDto>> Handle(
        GetDicasQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        // ── Mês atual ────────────────────────────────────────────────────────────
        var lancamentos = (await repository.GetByMesAnoAsync(
            request.Mes, request.Ano, userId, cancellationToken)).ToList();

        var creditos = lancamentos.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
        var debitos  = lancamentos.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);
        var saldo    = creditos - debitos;

        var topCategoria = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix)
            .GroupBy(l => l.Categoria?.Nome ?? "Sem Categoria")
            .Select(g => new { Nome = g.Key, Total = g.Sum(l => l.Valor) })
            .OrderByDescending(g => g.Total)
            .FirstOrDefault();

        // ── Mês anterior ─────────────────────────────────────────────────────────
        var mesAnt = request.Mes == 1 ? 12 : request.Mes - 1;
        var anoAnt = request.Mes == 1 ? request.Ano - 1 : request.Ano;
        var anteriores = (await repository.GetByMesAnoAsync(
            mesAnt, anoAnt, userId, cancellationToken)).ToList();

        var credAnt = anteriores.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
        var debAnt  = anteriores.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);

        // ── Métricas ─────────────────────────────────────────────────────────────
        decimal? comprometimento  = creditos > 0 ? Math.Round(debitos  / creditos * 100, 1) : null;
        decimal? variacaoDebitos  = debAnt  > 0  ? Math.Round((debitos  - debAnt)  / Math.Abs(debAnt)  * 100, 1) : null;
        decimal? variacaoCreditos = credAnt > 0  ? Math.Round((creditos - credAnt) / Math.Abs(credAnt) * 100, 1) : null;

        int? diasReserva = null;
        var saldoAcumulado = await repository.GetSaldoAcumuladoAsync(
            request.Mes, request.Ano, userId, cancellationToken);

        if (saldoAcumulado > 0)
        {
            var mes2 = mesAnt == 1 ? 12 : mesAnt - 1;
            var ano2 = mesAnt == 1 ? anoAnt - 1 : anoAnt;
            var doisMesesAtras = (await repository.GetByMesAnoAsync(mes2, ano2, userId, cancellationToken)).ToList();
            var totalDebitos3Meses = debitos + debAnt +
                doisMesesAtras.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);
            var gastoMedioDiario = totalDebitos3Meses / 90m;
            if (gastoMedioDiario > 0)
                diasReserva = (int)(saldoAcumulado / gastoMedioDiario);
        }

        // ── Tenta IA primeiro, fallback estático ─────────────────────────────────
        try
        {
            return await GerarComIaAsync(
                request.Mes, request.Ano,
                creditos, debitos, saldo,
                comprometimento, variacaoDebitos, variacaoCreditos,
                diasReserva, topCategoria?.Nome, topCategoria?.Total,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "IAiService falhou para dicas {Mes}/{Ano}. Usando lógica estática.", request.Mes, request.Ano);
            return GerarEstatico(creditos, debitos, saldo, comprometimento, variacaoDebitos, variacaoCreditos, diasReserva, topCategoria?.Nome, topCategoria?.Total, creditos);
        }
    }

    // ── Geração via IA ───────────────────────────────────────────────────────

    private async Task<IEnumerable<DicaFinanceiraDto>> GerarComIaAsync(
        int mes, int ano,
        decimal creditos, decimal debitos, decimal saldo,
        decimal? comprometimento, decimal? varDebitos, decimal? varCreditos,
        int? diasReserva, string? topCatNome, decimal? topCatTotal,
        CancellationToken ct)
    {
        const string systemPrompt = """
            Você é um analista financeiro pessoal experiente e empático.
            Analise os dados do mês e retorne APENAS um array JSON válido (sem markdown, sem texto extra)
            com até 3 dicas financeiras priorizadas por criticidade (mais crítico primeiro).

            Formato obrigatório de cada objeto:
            {"tipo":"critico|atencao|positivo","titulo":"texto curto (máx 40 chars)","descricao":"dica prática e motivadora (máx 130 chars)","acaoLabel":"texto ou null","acaoRota":"Lancamentos|Orcamento ou null"}

            Regras:
            - "critico" → problema urgente que exige ação imediata
            - "atencao" → ponto que merece acompanhamento
            - "positivo" → conquista ou situação favorável
            - acaoRota só pode ser "Lancamentos", "Orcamento" ou null
            - Responda APENAS com o JSON array, sem nenhum outro texto
            """;

        var compStr  = comprometimento.HasValue ? $"{comprometimento:F0}% das receitas" : "não calculado (sem receitas)";
        var varDebStr = varDebitos.HasValue  ? $"{(varDebitos > 0 ? "+" : "")}{varDebitos:F0}% vs mês passado" : "sem dados anteriores";
        var varCredStr = varCreditos.HasValue ? $"{(varCreditos > 0 ? "+" : "")}{varCreditos:F0}% vs mês passado" : "sem dados anteriores";
        var reservaStr = diasReserva.HasValue  ? $"{diasReserva} dias de cobertura" : "sem saldo acumulado";
        var topCatStr  = topCatNome is not null
            ? $"{topCatNome} — {Brl(topCatTotal ?? 0)}{(comprometimento.HasValue && comprometimento > 0 ? $" ({(topCatTotal ?? 0) / (creditos > 0 ? creditos : 1) * 100:F0}% da renda)" : "")}"
            : "sem despesas registradas";

        var userMessage = $"""
            Dados financeiros de {Meses[mes - 1]}/{ano}:
            - Receitas:                {Brl(creditos)}
            - Despesas:                {Brl(debitos)}
            - Saldo do mês:            {Brl(saldo)} ({(saldo >= 0 ? "positivo" : "negativo")})
            - Comprometimento de renda: {compStr}
            - Variação de despesas:    {varDebStr}
            - Variação de receitas:    {varCredStr}
            - Reserva de emergência:   {reservaStr}
            - Maior categoria de gasto: {topCatStr}

            Gere até 3 dicas priorizadas por criticidade.
            """;

        var raw  = await ai.ChatAsync(systemPrompt, userMessage, maxTokens: 600, temperature: 0.4f, ct);
        var json = ExtrairJson(raw);

        var dicas = JsonSerializer.Deserialize<List<DicaFinanceiraDto>>(json, _jsonOpts)
            ?? throw new InvalidOperationException("IA retornou JSON vazio.");

        if (dicas.Count == 0)
            throw new InvalidOperationException("IA não retornou nenhuma dica.");

        logger.LogInformation("IAiService gerou {Count} dica(s) para {Mes}/{Ano}", dicas.Count, mes, ano);
        return dicas.Take(3);
    }

    /// <summary>Remove ```json ... ``` caso a IA embrulhe o JSON em markdown.</summary>
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

    // ── Fallback estático ────────────────────────────────────────────────────

    private static IEnumerable<DicaFinanceiraDto> GerarEstatico(
        decimal creditos, decimal debitos, decimal saldo,
        decimal? comprometimento, decimal? varDebitos, decimal? varCreditos,
        int? diasReserva, string? topCatNome, decimal? topCatTotal, decimal rendaBase)
    {
        var dicas = new List<DicaFinanceiraDto>();

        if (comprometimento > 90)
            dicas.Add(new("critico",
                "Comprometimento Crítico",
                $"Gastos comprometem {comprometimento:F0}% da renda. Revise as maiores despesas para recuperar folga.",
                "Ver Orçamento", "Orcamento"));

        else if (saldo < 0)
            dicas.Add(new("critico",
                "Saldo Negativo",
                $"Despesas superaram receitas em {Brl(Math.Abs(saldo))}. Evite novos gastos não essenciais.",
                "Ver Lançamentos", "Lancamentos"));

        else if (varDebitos > 20)
            dicas.Add(new("atencao",
                "Despesas em Alta",
                $"Gastos subiram {varDebitos:F0}% vs mês passado. Compare os lançamentos para identificar o que mudou.",
                "Ver Lançamentos", "Lancamentos"));

        else if (comprometimento > 70)
            dicas.Add(new("atencao",
                "Renda Bastante Comprometida",
                $"{comprometimento:F0}% da renda comprometida. Mantenha abaixo de 70% para ter mais segurança.",
                "Ver Orçamento", "Orcamento"));

        else if (diasReserva < 30)
            dicas.Add(new("atencao",
                "Reserva de Emergência Baixa",
                $"Só {diasReserva} dias de cobertura — abaixo do mínimo recomendado (30 dias). Guarde parte das receitas.",
                null, null));

        else if (varCreditos > 10)
            dicas.Add(new("positivo",
                "Receitas Crescendo 🎉",
                $"Receitas cresceram {varCreditos:F0}% vs mês passado. Direcione o excedente à reserva.",
                null, null));

        else if (comprometimento <= 50 && saldo > 0)
            dicas.Add(new("positivo",
                "Ótimo Controle Financeiro",
                $"Só {comprometimento:F0}% da renda comprometida. Espaço para investir ou reforçar a reserva.",
                null, null));

        else
            dicas.Add(new("positivo",
                "Finanças Equilibradas",
                "Receitas e despesas bem balanceadas. Continue assim e considere metas de economia.",
                null, null));

        // Dicas secundárias
        if (topCatNome is not null && rendaBase > 0 && (topCatTotal ?? 0) > rendaBase * 0.30m && dicas[0].Tipo != "critico")
        {
            var pct = Math.Round((topCatTotal ?? 0) / rendaBase * 100, 0);
            dicas.Add(new("atencao",
                $"Destaque: {topCatNome}",
                $"\"{topCatNome}\" representa {pct}% da renda. Avalie se está dentro do esperado.",
                "Ver Orçamento", "Orcamento"));
        }

        if (diasReserva.HasValue && diasReserva < 90 && !dicas.Any(d => d.Titulo.Contains("Reserva")))
        {
            var tipo = diasReserva < 30 ? "critico" : "atencao";
            dicas.Add(new(tipo,
                $"Reserva: {diasReserva}d",
                diasReserva < 30
                    ? $"Só {diasReserva} dias de cobertura. Priorize a reserva antes de qualquer investimento."
                    : $"{diasReserva} dias de cobertura. O ideal é ter ao menos 90 dias guardados.",
                null, null));
        }

        return dicas.Take(3);
    }

    private static string Brl(decimal valor) =>
        valor.ToString("C", new CultureInfo("pt-BR"));
}
