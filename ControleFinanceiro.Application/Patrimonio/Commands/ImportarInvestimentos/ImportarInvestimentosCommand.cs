using System.Globalization;
using System.Text;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.ImportarInvestimentos;

public record ImportarInvestimentosResult(int Importados, IEnumerable<string> Erros);

/// <summary>
/// Importa investimentos a partir de um CSV (cabeçalho + linhas). Colunas aceitas
/// (ordem livre, sem acento/maiúsculas): nome, tipo, corretora, ticker, valoraplicado,
/// valoratual, moeda, rentabilidade. Cria os investimentos do usuário efetivo.
/// </summary>
public record ImportarInvestimentosCommand(string Conteudo) : IRequest<ImportarInvestimentosResult>;

public class ImportarInvestimentosCommandHandler(
    IInvestimentoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ImportarInvestimentosCommand, ImportarInvestimentosResult>
{
    public async Task<ImportarInvestimentosResult> Handle(ImportarInvestimentosCommand request, CancellationToken cancellationToken)
    {
        var erros = new List<string>();
        var novos = new List<Investimento>();

        var linhas = (request.Conteudo ?? "")
            .Replace("\r\n", "\n").Replace("\r", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (linhas.Length < 2)
            return new ImportarInvestimentosResult(0, new[] { "Arquivo vazio ou sem linhas de dados." });

        var sep = linhas[0].Contains(';') ? ';' : ',';
        var cols = linhas[0].Split(sep).Select(NormalizarCabecalho).ToList();
        int Idx(params string[] nomes) => cols.FindIndex(c => nomes.Contains(c));

        int iNome = Idx("nome", "ativo", "descricao"),
            iTipo = Idx("tipo", "classe"),
            iCorr = Idx("corretora", "instituicao", "custodiante"),
            iTick = Idx("ticker", "codigo", "papel"),
            iApl  = Idx("valoraplicado", "aplicado", "custo", "valorinvestido"),
            iAtu  = Idx("valoratual", "atual", "saldo", "posicao"),
            iMoe  = Idx("moeda"),
            iRent = Idx("rentabilidade", "rentabilidadeanual", "roi");

        if (iNome < 0 || iAtu < 0)
            return new ImportarInvestimentosResult(0, new[] { "Cabeçalho precisa ter ao menos as colunas 'nome' e 'valoratual'." });

        for (int i = 1; i < linhas.Length; i++)
        {
            var campos = linhas[i].Split(sep);
            string Get(int idx) => idx >= 0 && idx < campos.Length ? campos[idx].Trim() : "";

            var nome = Get(iNome);
            if (string.IsNullOrWhiteSpace(nome)) { erros.Add($"Linha {i + 1}: nome vazio."); continue; }

            var valorAtual = ParseDecimal(Get(iAtu));
            if (valorAtual is null) { erros.Add($"Linha {i + 1} ({nome}): valor atual inválido."); continue; }

            var valorAplicado = ParseDecimal(Get(iApl)) ?? valorAtual.Value;
            var rent = ParseDecimal(Get(iRent));

            novos.Add(new Investimento(
                currentUser.UserId, nome,
                ParseTipo(Get(iTipo)), ParseMoeda(Get(iMoe)),
                string.IsNullOrWhiteSpace(Get(iCorr)) ? null : Get(iCorr),
                string.IsNullOrWhiteSpace(Get(iTick)) ? null : Get(iTick),
                valorAplicado, valorAtual.Value, rent));
        }

        foreach (var inv in novos)
            await repository.AddAsync(inv, cancellationToken);
        if (novos.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportarInvestimentosResult(novos.Count, erros);
    }

    private static string NormalizarCabecalho(string s)
    {
        s = s.Trim().ToLowerInvariant();
        var semAcento = string.Concat(s.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        return new string(semAcento.Where(char.IsLetterOrDigit).ToArray());
    }

    private static decimal? ParseDecimal(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = new string(raw.Where(c => char.IsDigit(c) || c is '.' or ',' or '-').ToArray());
        if (s.Length == 0) return null;
        bool temPonto = s.Contains('.'), temVirgula = s.Contains(',');
        if (temPonto && temVirgula)
            s = s.LastIndexOf(',') > s.LastIndexOf('.')
                ? s.Replace(".", "").Replace(",", ".")   // BR: 1.234,56
                : s.Replace(",", "");                     // US: 1,234.56
        else if (temVirgula)
            s = s.Replace(",", ".");
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private static TipoInvestimento ParseTipo(string raw)
    {
        var t = NormalizarCabecalho(raw);
        return t switch
        {
            "acoes" or "acao" or "stocks" or "1" => TipoInvestimento.Acoes,
            "fii" or "fiis" or "fundoimobiliario" or "2" => TipoInvestimento.FII,
            "etf" or "etfs" or "3" => TipoInvestimento.ETF,
            "rendafixa" or "rf" or "4" => TipoInvestimento.RendaFixa,
            "multimercado" or "multi" or "5" => TipoInvestimento.Multimercado,
            "cripto" or "crypto" or "6" => TipoInvestimento.Cripto,
            "exterior" or "internacional" or "7" => TipoInvestimento.Exterior,
            _ => TipoInvestimento.Outro,
        };
    }

    private static MoedaPatrimonio ParseMoeda(string raw)
    {
        var m = raw.Trim().ToUpperInvariant();
        return m switch
        {
            "USD" or "US$" or "DOLAR" => MoedaPatrimonio.USD,
            "EUR" or "€" => MoedaPatrimonio.EUR,
            "CHF" => MoedaPatrimonio.CHF,
            "GBP" or "£" => MoedaPatrimonio.GBP,
            _ => MoedaPatrimonio.BRL,
        };
    }
}
