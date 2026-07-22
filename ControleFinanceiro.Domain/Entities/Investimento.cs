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
    /// <summary>Subclasse livre dentro do Tipo (classe). Ex.: "IPCA+", "Small Caps", "High Yield".</summary>
    public string? Subclasse { get; private set; }
    public string? Corretora { get; private set; }
    public string? Ticker { get; private set; }
    /// <summary>Quantidade de cotas/ações. Para posições com ticker, ValorAtual = Quantidade × preço unitário.</summary>
    public decimal? Quantidade { get; private set; }
    /// <summary>Estrutura à qual o investimento pertence (holding, offshore…). null = pessoa física.</summary>
    public Guid? EstruturaId { get; private set; }
    /// <summary>Conta de investimento/custódia à qual o investimento está vinculado. null = solto.</summary>
    public Guid? ContaId { get; private set; }
    public decimal ValorAplicado { get; private set; }
    public decimal ValorAtual { get; private set; }
    /// <summary>Rentabilidade anual estimada em %. Null = não informado.</summary>
    public decimal? RentabilidadeAnualPct { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }
    /// <summary>Data/hora UTC da última atualização automática do valor atual (cotação de preço).</summary>
    public DateTime? ValorAtualizadoEm { get; private set; }

    private Investimento() { }

    public Investimento(
        Guid usuarioId, string nome, TipoInvestimento tipo, MoedaPatrimonio moeda,
        string? corretora, string? ticker, decimal valorAplicado, decimal valorAtual,
        decimal? rentabilidadeAnualPct = null, decimal? quantidade = null, Guid? estruturaId = null,
        Guid? contaId = null, string? subclasse = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        Corretora = corretora;
        Ticker = ticker;
        Quantidade = quantidade;
        ValorAplicado = valorAplicado;
        ValorAtual = valorAtual;
        RentabilidadeAnualPct = rentabilidadeAnualPct;
        EstruturaId = estruturaId;
        ContaId = contaId;
        Subclasse = subclasse;
    }

    public void Atualizar(string nome, TipoInvestimento tipo, MoedaPatrimonio moeda,
        string? corretora, string? ticker, decimal valorAplicado, decimal valorAtual,
        decimal? rentabilidadeAnualPct, decimal? quantidade = null, Guid? estruturaId = null,
        Guid? contaId = null, string? subclasse = null)
    {
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        Corretora = corretora;
        Ticker = ticker;
        Quantidade = quantidade;
        ValorAplicado = valorAplicado;
        ValorAtual = valorAtual;
        RentabilidadeAnualPct = rentabilidadeAnualPct;
        EstruturaId = estruturaId;
        ContaId = contaId;
        Subclasse = subclasse;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Solta o investimento da estrutura (volta para pessoa física) — usado ao excluir a estrutura.</summary>
    public void DesvincularEstrutura()
    {
        EstruturaId = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Solta o investimento da conta de custódia — usado ao excluir a conta.</summary>
    public void DesvincularConta()
    {
        ContaId = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza o valor atual via cotação automática (preço UNITÁRIO do ativo).
    /// Só funciona quando há quantidade: ValorAtual = Quantidade × preço unitário.
    /// Retorna false (e não altera nada) quando não há quantidade — sem ela não dá para
    /// derivar o valor da posição a partir do preço por ação.
    /// </summary>
    public bool AtualizarValorAutomatico(decimal precoUnitario)
    {
        if (Quantidade is not > 0) return false;

        ValorAtual = Math.Round(Quantidade.Value * precoUnitario, 2);
        var agora = DateTime.UtcNow;
        AtualizadoEm = agora;
        ValorAtualizadoEm = agora;
        return true;
    }
}
