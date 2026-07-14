namespace ControleFinanceiro.Application.Relatorios;

/// <summary>
/// Cálculo da projeção patrimonial (acúmulo → decumulação), portado do motor do app.
/// Passo mensal com juros compostos sobre a taxa real anual + cenários extras.
/// </summary>
public static class ProjecaoSimulacaoCalc
{
    private const int IdadeMax = 100;

    public record Cenario(int Tipo, decimal Valor, int IdadeInicio, int? IdadeFim);

    /// <summary>Retorna (patrimônio na idade-alvo, idade de extinção ou null se sustentável).</summary>
    public static (decimal patrimonioNaIdadeAlvo, int? idadeExtincao) Calcular(
        int idadeAtual, int idadeAlvo, decimal patrimonioInicial,
        decimal aporteMensal, decimal taxaRetornoRealAnualPct, decimal retiradaMensal,
        IEnumerable<Cenario> cenarios)
    {
        var rm = (decimal)(Math.Pow(1 + (double)(taxaRetornoRealAnualPct / 100m), 1.0 / 12.0) - 1);
        var cen = cenarios.ToList();

        decimal saldo = patrimonioInicial;
        decimal patrimonioNaIdadeAlvo = patrimonioInicial;
        int? idadeExtincao = null;

        for (var idade = idadeAtual; idade < IdadeMax; idade++)
        {
            for (var mes = 0; mes < 12; mes++)
            {
                saldo *= 1 + rm;
                if (idade < idadeAlvo) saldo += aporteMensal;
                else                   saldo -= retiradaMensal;

                foreach (var c in cen)
                {
                    var aplica = c.IdadeFim == null
                        ? idade == c.IdadeInicio && mes == 0
                        : idade >= c.IdadeInicio && idade <= c.IdadeFim;
                    if (!aplica) continue;
                    saldo += c.Tipo == 1 ? c.Valor : -c.Valor;
                }

                if (saldo <= 0 && idade >= idadeAlvo && idadeExtincao == null)
                {
                    idadeExtincao = idade;
                    saldo = 0;
                }
            }

            if (idade + 1 == idadeAlvo) patrimonioNaIdadeAlvo = saldo;
            if (idadeExtincao != null) break;
        }

        if (idadeAlvo <= idadeAtual) patrimonioNaIdadeAlvo = patrimonioInicial;

        return (Math.Round(patrimonioNaIdadeAlvo, 2), idadeExtincao);
    }
}
