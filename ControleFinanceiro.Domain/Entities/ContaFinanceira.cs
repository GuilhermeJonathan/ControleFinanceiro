using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Conta financeira do cliente (bancária, de investimento/custódia ou internacional).
/// Contas-caixa (corrente/internacional) usam <see cref="Saldo"/> manual; contas de
/// investimento/custódia agregam os investimentos ligados (valor derivado na query).
/// Pode pertencer a uma estrutura (holding/offshore) ou ficar na pessoa física.
/// </summary>
public class ContaFinanceira
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoContaFinanceira Tipo { get; private set; }
    /// <summary>Banco/corretora/custodiante.</summary>
    public string? Instituicao { get; private set; }
    public string? Pais { get; private set; }
    public MoedaPatrimonio Moeda { get; private set; }
    /// <summary>Saldo manual (contas-caixa). Contas de investimento derivam o valor dos investimentos ligados.</summary>
    public decimal Saldo { get; private set; }
    /// <summary>Agência/conta ou número de custódia.</summary>
    public string? Identificador { get; private set; }
    /// <summary>Estrutura à qual a conta pertence (holding, offshore…). null = pessoa física.</summary>
    public Guid? EstruturaId { get; private set; }
    // ── Detalhes family-office (opcionais) ──────────────────────────────────
    /// <summary>Valor do portfólio investido na conta (separado do caixa).</summary>
    public decimal? ValorPortfolio { get; private set; }
    /// <summary>Crédito lombardo — limite total.</summary>
    public decimal? LombardLimite { get; private set; }
    /// <summary>Crédito lombardo — valor utilizado.</summary>
    public decimal? LombardUtilizado { get; private set; }
    /// <summary>Status livre. Ex.: "Ativa", "Pré-aprovada", "Em revisão".</summary>
    public string? Status { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private ContaFinanceira() { }

    public ContaFinanceira(
        Guid usuarioId, string nome, TipoContaFinanceira tipo, MoedaPatrimonio moeda, decimal saldo,
        string? instituicao = null, string? pais = null, string? identificador = null, Guid? estruturaId = null,
        decimal? valorPortfolio = null, decimal? lombardLimite = null, decimal? lombardUtilizado = null, string? status = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        Saldo = saldo;
        Instituicao = instituicao;
        Pais = pais;
        Identificador = identificador;
        EstruturaId = estruturaId;
        ValorPortfolio = valorPortfolio;
        LombardLimite = lombardLimite;
        LombardUtilizado = lombardUtilizado;
        Status = status;
    }

    public void Atualizar(string nome, TipoContaFinanceira tipo, MoedaPatrimonio moeda, decimal saldo,
        string? instituicao, string? pais, string? identificador, Guid? estruturaId,
        decimal? valorPortfolio = null, decimal? lombardLimite = null, decimal? lombardUtilizado = null, string? status = null)
    {
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        Saldo = saldo;
        Instituicao = instituicao;
        Pais = pais;
        Identificador = identificador;
        EstruturaId = estruturaId;
        ValorPortfolio = valorPortfolio;
        LombardLimite = lombardLimite;
        LombardUtilizado = lombardUtilizado;
        Status = status;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Solta a conta da estrutura (volta para pessoa física) — usado ao excluir a estrutura.</summary>
    public void DesvincularEstrutura()
    {
        EstruturaId = null;
        AtualizadoEm = DateTime.UtcNow;
    }
}
