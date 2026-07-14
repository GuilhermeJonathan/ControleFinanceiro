using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Dívida/passivo do patrimônio de um usuário (módulo de gestão patrimonial — B2B alta renda).
/// Compõe o balanço patrimonial: Bens − Dívidas = Patrimônio Líquido.
/// Bounded context isolado: NÃO se mistura com Lancamentos/Orçamento do FinDog pessoal.
/// </summary>
public class PassivoPatrimonial
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public MoedaPatrimonio Moeda { get; private set; }
    /// <summary>Saldo devedor atual na moeda da dívida.</summary>
    public decimal Valor { get; private set; }
    public PrazoDivida Prazo { get; private set; }
    /// <summary>Taxa de juros anual em % (para projeção de amortização). Null = sem juros informados.</summary>
    public decimal? TaxaJurosAnualPct { get; private set; }
    /// <summary>Prazo de quitação em meses (para projeção). Null = sem cronograma (bullet).</summary>
    public int? PrazoMeses { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private PassivoPatrimonial() { }

    public PassivoPatrimonial(
        Guid usuarioId, string nome, MoedaPatrimonio moeda, decimal valor,
        PrazoDivida prazo, decimal? taxaJurosAnualPct = null, int? prazoMeses = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Moeda = moeda;
        Valor = valor;
        Prazo = prazo;
        TaxaJurosAnualPct = taxaJurosAnualPct;
        PrazoMeses = prazoMeses;
    }

    public void Atualizar(string nome, MoedaPatrimonio moeda, decimal valor,
        PrazoDivida prazo, decimal? taxaJurosAnualPct, int? prazoMeses)
    {
        Nome = nome;
        Moeda = moeda;
        Valor = valor;
        Prazo = prazo;
        TaxaJurosAnualPct = taxaJurosAnualPct;
        PrazoMeses = prazoMeses;
        AtualizadoEm = DateTime.UtcNow;
    }
}
