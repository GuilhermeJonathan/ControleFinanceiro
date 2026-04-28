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

    // ── Regex: captura valor monetário ───────────────────────────────────────
    // Aceita: 300 / 300,50 / 300.50 / R$300 / R$ 300,50
    [GeneratedRegex(@"R?\$\s*(\d+(?:[.,]\d{1,2})?)|(?<!\d)(\d+(?:[.,]\d{1,2})?)(?!\d)(?:\s*reais?)?",
        RegexOptions.IgnoreCase)]
    private static partial Regex ValueRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();

    // ── Comandos especiais ────────────────────────────────────────────────────
    public static bool IsCommand(string text, out string reply)
    {
        var lower = text.Trim().ToLowerInvariant();
        if (lower is "ajuda" or "help" or "?")
        {
            reply = """
                🐾 *Meu Financeiro — como usar*

                Envie uma mensagem com a descrição e o valor:

                *Exemplos de despesa:*
                • Gasolina 300 reais
                • Almoço 45,50
                • Mercado ontem 230

                *Exemplos de receita:*
                • Recebi salário 5000
                • Renda aluguel 1200

                ℹ️ Se não informar a data, registra como *hoje*.
                Palavras aceitas para data: _hoje_, _ontem_, _amanhã_.
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
        var data = lower.Contains("ontem")                     ? DateTime.Today.AddDays(-1)
                 : lower.Contains("amanhã") || lower.Contains("amanha") ? DateTime.Today.AddDays(1)
                 : DateTime.Today;

        // 4. Descrição — remove valor, data, indicadores e ruído
        var desc = text;
        desc = ValueRegex().Replace(desc, " ");

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
}
