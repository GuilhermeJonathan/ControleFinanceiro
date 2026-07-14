using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Investimento financeiro do usuário (módulo patrimonial B2B).
/// Bounded context isolado de Lancamentos/Orcamento.
/// </summary>
public class Investimento
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoInvestimento Tipo { get; private set; }
    public MoedaPatrimonio Moeda { get; private set; }
    public string? Corretora { get; private set; }
    public string? Ticker { get; private set; }
    public decimal ValorAplicado { get; private set; }
    public decimal ValorAtual { get; private set; }
    /// <summary>Rentabilidade anual estimada em %. Null = não informado.</summary>
    public decimal? RentabilidadeAnualPct { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private Investimento() { }

    public Investimento(
        Guid usuarioId, string nome, TipoInvestimento tipo, MoedaPatrimonio moeda,
        string? corretora, string? ticker, decimal valorAplicado, decimal valorAtual,
        decimal? rentabilidadeAnualPct = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        Corretora = corretora;
        Ticker = ticker;
        ValorAplicado = valorAplicado;
        ValorAtual = valorAtual;
        RentabilidadeAnualPct = rentabilidadeAnualPct;
    }

    public void Atualizar(string nome, TipoInvestimento tipo, MoedaPatrimonio moeda,
        string? corretora, string? ticker, decimal valorAplicado, decimal valorAtual,
        decimal? rentabilidadeAnualPct)
    {
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        Corretora = corretora;
        Ticker = ticker;
        ValorAplicado = valorAplicado;
        ValorAtual = valorAtual;
        RentabilidadeAnualPct = rentabilidadeAnualPct;
        AtualizadoEm = DateTime.UtcNow;
    }
}
