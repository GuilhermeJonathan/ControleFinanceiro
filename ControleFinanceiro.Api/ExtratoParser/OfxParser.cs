using System.Text.RegularExpressions;

namespace ControleFinanceiro.Api.ExtratoParser;

public record OfxTransacao(
    string   Id,
    decimal  Valor,
    DateTime Data,
    string   Memo);

public static partial class OfxParser
{
    // OFX é SGML — não é XML válido, então usamos regex
    [GeneratedRegex(@"<STMTTRN>(.*?)</STMTTRN>", RegexOptions.Singleline)]
    private static partial Regex TrxBlockRegex();

    [GeneratedRegex(@"<FITID>(.*?)\n|<FITID>(.*?)<")]
    private static partial Regex FitidRegex();

    [GeneratedRegex(@"<TRNAMT>(.*?)\n|<TRNAMT>(.*?)<")]
    private static partial Regex AmtRegex();

    [GeneratedRegex(@"<DTPOSTED>(.*?)\n|<DTPOSTED>(.*?)<")]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"<MEMO>(.*?)\n|<MEMO>(.*?)<")]
    private static partial Regex MemoRegex();

    [GeneratedRegex(@"<NAME>(.*?)\n|<NAME>(.*?)<")]
    private static partial Regex NameRegex();

    public static List<OfxTransacao> Parse(string content)
    {
        var result = new List<OfxTransacao>();

        foreach (Match block in TrxBlockRegex().Matches(content))
        {
            var body = block.Groups[1].Value;

            var fitid  = ExtractField(FitidRegex(), body) ?? Guid.NewGuid().ToString();
            var amtStr = ExtractField(AmtRegex(), body) ?? "0";
            var dateStr = ExtractField(DateRegex(), body) ?? "";
            var memo   = ExtractField(MemoRegex(), body)
                      ?? ExtractField(NameRegex(), body)
                      ?? "Lançamento";

            if (!decimal.TryParse(amtStr.Replace(",", ".").Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var valor)) continue;

            var data = ParseOfxDate(dateStr);
            if (data == DateTime.MinValue) continue;

            result.Add(new OfxTransacao(fitid.Trim(), valor, data, memo.Trim()));
        }

        return result;
    }

    private static string? ExtractField(Regex rx, string body)
    {
        var m = rx.Match(body);
        if (!m.Success) return null;
        return (m.Groups[1].Success ? m.Groups[1] : m.Groups[2]).Value.Trim();
    }

    private static DateTime ParseOfxDate(string raw)
    {
        // OFX date: YYYYMMDD or YYYYMMDDHHMMSS or YYYYMMDDHHMMSS.XXX[-TZ:Nome]
        var digits = raw.Replace(".", "").Split('[')[0].Split('-')[0].Trim();
        if (digits.Length >= 8 &&
            int.TryParse(digits[..4], out var y) &&
            int.TryParse(digits[4..6], out var mo) &&
            int.TryParse(digits[6..8], out var d))
        {
            try { return new DateTime(y, mo, d, 0, 0, 0, DateTimeKind.Utc); }
            catch { return DateTime.MinValue; }
        }
        return DateTime.MinValue;
    }
}
