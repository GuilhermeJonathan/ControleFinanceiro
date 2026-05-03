using System.Text.RegularExpressions;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Api.WhatsApp;

public record ParsedLancamento(
    bool    Success,
    string  Descricao    = "",
    decimal Valor        = 0,
    DateTime Data        = default,
    TipoLancamento Tipo  = TipoLancamento.Debito,
    string? Erro         = null);

public static partial class WhatsAppMessageParser
{
    // ── Classificação de tipo ────────────────────────────────────────────────
    private static readonly string[] _creditWords =
        ["recebi", "salario", "salário", "renda", "receita",
         "ganhei", "depositei", "recebimento", "entrada"];

    // Removidos da descrição (são apenas indicadores, não nomes)
    private static readonly string[] _creditIndicators =
        ["recebi", "ganhei", "depositei"];

    // ── Palavras de data ─────────────────────────────────────────────────────
    private static readonly string[] _dateWords =
        ["hoje", "ontem", "amanhã", "amanha"];

    // ── Unidades monetárias — removidas da descrição ─────────────────────────
    private static readonly string[] _noiseWords =
        ["reais", "real"];

    // ── Helpers de data ──────────────────────────────────────────────────────
    /// <summary>
    /// Retorna a data de hoje no horário de Brasília (UTC-3), armazenada como
    /// 03:00 UTC — que equivale à meia-noite de Brasília — para que o app mobile
    /// (que converte UTC para local) exiba o dia correto.
    /// </summary>
    public static DateTime TodayBrazil()
    {
        var brazilDate = DateTime.UtcNow.AddHours(-3).Date; // sem DST: Brasília = UTC-3
        return new DateTime(brazilDate.Year, brazilDate.Month, brazilDate.Day, 3, 0, 0, DateTimeKind.Utc);
    }

    // ── Regex: captura valor monetário ───────────────────────────────────────
    // Aceita: 300 / 300,50 / 300.50 / R$300 / R$ 300,50
    [GeneratedRegex(@"R?\$\s*(\d+(?:[.,]\d{1,2})?)|(?<!\d)(\d+(?:[.,]\d{1,2})?)(?!\d)(?:\s*reais?)?",
        RegexOptions.IgnoreCase)]
    private static partial Regex ValueRegex();

    // Aceita: 15/04 | 15/04/2025 | 15-04 | 15-04-2025
    [GeneratedRegex(@"\b(\d{1,2})[/\-](\d{1,2})(?:[/\-](\d{2,4}))?\b")]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();

    // ── Comandos especiais ────────────────────────────────────────────────────
    public static bool IsCommand(string text, out string reply)
    {
        var lower = text.Trim().ToLowerInvariant();
        if (lower is "ajuda" or "help" or "?")
        {
            reply = """
                🐾 *Meu FinDog — como usar*

                Envie uma mensagem com a descrição e o valor:

                *Exemplos de despesa:*
                • Gasolina 300 reais
                • Almoço 45,50
                • Mercado ontem 230

                *Exemplos de receita:*
                • Recebi salário 5000
                • Renda aluguel 1200

                ℹ️ Se não informar a data, registra como *hoje*.
                Palavras aceitas: _hoje_, _ontem_, _amanhã_.
                Ou informe a data: _15/04_ ou _15/04/2025_.
                """;
            return true;
        }

        reply = "";
        return false;
    }

    // ── Parser principal ──────────────────────────────────────────────────────
    public static ParsedLancamento Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new(false, Erro: "Mensagem vazia.");

        var lower = text.Trim().ToLowerInvariant();

        // 1. Tipo
        var tipo = _creditWords.Any(lower.Contains)
            ? TipoLancamento.Credito
            : TipoLancamento.Debito;

        // 2. Valor
        var match = ValueRegex().Match(text);
        if (!match.Success)
            return new(false, Erro:
                "Não consegui identificar o valor 🤔\nTente: *Gasolina 150 reais*");

        var rawValue = (match.Groups[1].Success ? match.Groups[1] : match.Groups[2]).Value
            .Replace(",", ".");

        if (!decimal.TryParse(rawValue,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var valor) || valor <= 0)
            return new(false, Erro:
                "Valor inválido 🤔\nTente: *Gasolina 150 reais*");

        // 3. Data
        DateTime data;
        var today     = TodayBrazil();
        var dateMatch = DateRegex().Match(text);
        if (lower.Contains("ontem"))
            data = today.AddDays(-1);
        else if (lower.Contains("amanhã") || lower.Contains("amanha"))
            data = today.AddDays(1);
        else if (dateMatch.Success &&
                 int.TryParse(dateMatch.Groups[1].Value, out var dia) &&
                 int.TryParse(dateMatch.Groups[2].Value, out var mes))
        {
            var ano = today.Year;
            if (dateMatch.Groups[3].Success && int.TryParse(dateMatch.Groups[3].Value, out var anoRaw))
                ano = anoRaw < 100 ? 2000 + anoRaw : anoRaw;
            data = IsValidDate(dia, mes, ano)
                ? new DateTime(ano, mes, dia, 3, 0, 0, DateTimeKind.Utc)
                : today;
        }
        else
            data = today;

        // 4. Descrição — remove valor, data, indicadores e ruído
        var desc = text;
        desc = ValueRegex().Replace(desc, " ");
        desc = DateRegex().Replace(desc, " ");   // remove data explícita da descrição

        var wordsToRemove = _dateWords
            .Concat(_noiseWords)
            .Concat(_creditIndicators);

        foreach (var w in wordsToRemove)
            desc = Regex.Replace(desc, $@"\b{Regex.Escape(w)}\b", " ", RegexOptions.IgnoreCase);

        desc = SpaceRegex().Replace(desc, " ").Trim();

        if (string.IsNullOrWhiteSpace(desc))
            desc = tipo == TipoLancamento.Credito ? "Receita" : "Despesa";
        else
            desc = char.ToUpper(desc[0]) + desc[1..];

        return new(true, desc, valor, data, tipo);
    }

    private static bool IsValidDate(int dia, int mes, int ano)
    {
        try { _ = new DateTime(ano, mes, dia); return true; }
        catch { return false; }
    }
}
