using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Ativo do patrimônio de um usuário (módulo de gestão patrimonial — B2B alta renda).
/// Bounded context isolado: NÃO se mistura com Lancamentos/Orçamento do FinDog pessoal.
/// </summary>
public class AtivoPatrimonial
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoAtivo Tipo { get; private set; }
    public MoedaPatrimonio Moeda { get; private set; }
    public decimal ValorAtual { get; private set; }
    /// <summary>Valorização (ou depreciação, se negativa) anual estimada em %. Null = não informado.</summary>
    public decimal? ValorizacaoAnualPct { get; private set; }
    /// <summary>Receita mensal que o bem gera (aluguel, dividendos, pró-labore…). 0 = não gera renda.</summary>
    public decimal ReceitaMensal { get; private set; }
    /// <summary>Despesa mensal atrelada ao bem (condomínio, manutenção, IPTU rateado…). 0 = sem custo.</summary>
    public decimal DespesaMensal { get; private set; }
    /// <summary>Estrutura à qual o bem pertence (holding, trust…). null = pessoa física.</summary>
    public Guid? EstruturaId { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private AtivoPatrimonial() { }

    public AtivoPatrimonial(
        Guid usuarioId, string nome, TipoAtivo tipo, MoedaPatrimonio moeda,
        decimal valorAtual, decimal? valorizacaoAnualPct = null,
        decimal receitaMensal = 0m, decimal despesaMensal = 0m, Guid? estruturaId = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        ValorAtual = valorAtual;
        ValorizacaoAnualPct = valorizacaoAnualPct;
        ReceitaMensal = receitaMensal;
        DespesaMensal = despesaMensal;
        EstruturaId = estruturaId;
    }

    public void Atualizar(string nome, TipoAtivo tipo, MoedaPatrimonio moeda,
        decimal valorAtual, decimal? valorizacaoAnualPct,
        decimal receitaMensal = 0m, decimal despesaMensal = 0m, Guid? estruturaId = null)
    {
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        ValorAtual = valorAtual;
        ValorizacaoAnualPct = valorizacaoAnualPct;
        ReceitaMensal = receitaMensal;
        DespesaMensal = despesaMensal;
        EstruturaId = estruturaId;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Solta o bem da estrutura (volta para pessoa física) — usado ao excluir a estrutura.</summary>
    public void DesvincularEstrutura()
    {
        EstruturaId = null;
        AtualizadoEm = DateTime.UtcNow;
    }
}
