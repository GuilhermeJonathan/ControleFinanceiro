using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;

/// <summary>
/// Calcula os scores (0–100) de Governança e Conformidade da sucessão a partir dos dados
/// cadastrados. São heurísticas objetivas; o assessor pode sobrescrever manualmente.
/// </summary>
public static class IndicadoresSucessaoCalc
{
    public static (int Governanca, int Conformidade) Calcular(
        GrafoEstruturasDto grafo, SucessaoDto sucessao, ContasResultDto contas, IReadOnlyList<PlanoAcaoDto> planos)
    {
        var estruturas = grafo.Estruturas;
        var benef = sucessao.Beneficiarios;
        var distribs = sucessao.Distribuicoes;
        var totalGeral = grafo.TotalEmEstruturasBRL + grafo.TotalPessoaFisicaBRL;

        static int Pct(int parte, int total) => total <= 0 ? 0 : (int)Math.Round(100.0 * parte / total);

        // ── Governança: média de 5 dimensões (presença/organização) ──
        var comJurisdicao = estruturas.Count > 0 ? Pct(estruturas.Count(e => !string.IsNullOrWhiteSpace(e.Jurisdicao)), estruturas.Count) : 0;
        var comConstituicao = estruturas.Count > 0 ? Pct(estruturas.Count(e => e.ConstituidaEm != null), estruturas.Count) : 0;
        var temPlano = planos.Any(p => p.Etapas.Any()) ? 100 : 0;
        var temBenef = benef.Count > 0 ? 100 : 0;
        var alocacao = totalGeral > 0 ? (int)Math.Round(100.0 * (double)(grafo.TotalEmEstruturasBRL / totalGeral)) : 0;
        var governanca = (int)Math.Round((comJurisdicao + comConstituicao + temPlano + temBenef + alocacao) / 5.0);

        // ── Conformidade: média dos checks aplicáveis (higiene de dados) ──
        var checks = new List<int>();
        if (benef.Count > 0)
        {
            var soma = benef.Sum(b => b.PercentualDistribuicao);
            checks.Add(Math.Abs(soma - 100m) <= 1m ? 100 : (int)Math.Max(0, 100 - Math.Abs(100m - soma)));
        }
        if (distribs.Count > 0)
            checks.Add(Pct(distribs.Count(d => d.BeneficiarioId != null), distribs.Count));
        if (estruturas.Count > 0)
            checks.Add(comJurisdicao);
        var internacionais = contas.Contas.Where(c => c.Tipo == 3).ToList();
        if (internacionais.Count > 0)
            checks.Add(Pct(internacionais.Count(c => !string.IsNullOrWhiteSpace(c.Pais) && !string.IsNullOrWhiteSpace(c.Identificador)), internacionais.Count));

        var conformidade = checks.Count > 0 ? (int)Math.Round(checks.Average()) : 0;

        return (Math.Clamp(governanca, 0, 100), Math.Clamp(conformidade, 0, 100));
    }
}
