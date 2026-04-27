using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using ControleFinanceiro.Application.Faturas;

namespace ControleFinanceiro.Api.Services;

public static class FaturaParser
{
    private static readonly Regex DateRegex   = new(@"^\d{2}/\d{2}$",          RegexOptions.Compiled);
    private static readonly Regex ParcelRegex = new(@"Parc\.(\d+)/(\d+)",       RegexOptions.Compiled);
    private static readonly Regex ValueRegex  = new(@"R\$\s*([\d.,]+)",         RegexOptions.Compiled);
    private static readonly Regex CardRegex   = new(
        @"(\d{4})\s{1,}([A-ZÁÉÍÓÚÃÕÇÂÊÎÔÛÀÜ][A-ZÁÉÍÓÚÃÕÇÂÊÎÔÛÀÜ\s]+)$",
        RegexOptions.Compiled);

    public static List<FaturaTransacaoDto> Parse(byte[] excelBytes, int mesFatura, int anoFatura)
    {
        var result = new List<FaturaTransacaoDto>();

        using var ms = new MemoryStream(excelBytes);
        using var wb = new XLWorkbook(ms);

        var ws = wb.Worksheets.FirstOrDefault(w =>
                     w.Name.Contains("Lan", StringComparison.OrdinalIgnoreCase))
                 ?? wb.Worksheets.First();

        // Detecta se existe coluna E (Categoria) inspecionando o header
        bool temColCategoria = false;
        var secaoCartao   = "????";
        var titularCartao = "";

        foreach (var row in ws.RowsUsed())
        {
            var cellA = row.Cell(1).GetString().Trim();
            var cellB = row.Cell(2).GetString().Trim();
            var cellC = row.Cell(3).GetString().Trim();
            var cellD = row.Cell(4).GetString().Trim();
            var cellE = row.Cell(5).GetString().Trim();

            // Detecta linha de header para saber se há coluna Categoria
            if (cellA.Equals("DATA", StringComparison.OrdinalIgnoreCase))
            {
                temColCategoria = cellE.Length > 0;
                continue;
            }

            // Cabeçalho de seção de cartão
            if (cellD.StartsWith("Total: R$", StringComparison.OrdinalIgnoreCase))
            {
                var m = CardRegex.Match((cellA + " " + cellB).Trim());
                if (m.Success)
                {
                    secaoCartao   = m.Groups[1].Value;
                    titularCartao = m.Groups[2].Value.Trim();
                }
                continue;
            }

            // Apenas linhas com data DD/MM
            if (!DateRegex.IsMatch(cellA)) continue;

            // Pula créditos/ajustes negativos
            if (cellD.TrimStart().StartsWith("-")) continue;

            // Parse da data
            var parts     = cellA.Split('/');
            var dia       = int.Parse(parts[0]);
            var mesCompra = int.Parse(parts[1]);
            var anoCompra = mesCompra > mesFatura ? anoFatura - 1 : anoFatura;
            var diaMax    = DateTime.DaysInMonth(anoCompra, mesCompra);
            var data      = new DateTime(anoCompra, mesCompra, Math.Min(dia, diaMax));

            // Parse do valor
            var vm = ValueRegex.Match(cellD);
            if (!vm.Success) continue;
            var valor = decimal.Parse(
                vm.Groups[1].Value.Replace(".", "").Replace(",", "."),
                CultureInfo.InvariantCulture);
            if (valor <= 0) continue;

            // Parse de parcelas
            int? parcelaAtual  = null;
            int? totalParcelas = null;
            var pm = ParcelRegex.Match(cellC);
            if (pm.Success)
            {
                parcelaAtual  = int.Parse(pm.Groups[1].Value);
                totalParcelas = int.Parse(pm.Groups[2].Value);
            }

            // Categoria: col E se existir, senão "Outros"
            var categoria = temColCategoria && cellE.Length > 0 ? cellE : "Outros";

            result.Add(new FaturaTransacaoDto(
                Descricao:     cellB,
                Data:          data,
                Valor:         valor,
                Mes:           mesFatura,
                Ano:           anoFatura,
                ParcelaAtual:  parcelaAtual,
                TotalParcelas: totalParcelas,
                SecaoCartao:   secaoCartao,
                TitularCartao: titularCartao,
                CategoriaNome: categoria));
        }

        return result;
    }
}
