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
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private AtivoPatrimonial() { }

    public AtivoPatrimonial(
        Guid usuarioId, string nome, TipoAtivo tipo, MoedaPatrimonio moeda,
        decimal valorAtual, decimal? valorizacaoAnualPct = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        ValorAtual = valorAtual;
        ValorizacaoAnualPct = valorizacaoAnualPct;
    }

    public void Atualizar(string nome, TipoAtivo tipo, MoedaPatrimonio moeda,
        decimal valorAtual, decimal? valorizacaoAnualPct)
    {
        Nome = nome;
        Tipo = tipo;
        Moeda = moeda;
        ValorAtual = valorAtual;
        ValorizacaoAnualPct = valorizacaoAnualPct;
        AtualizadoEm = DateTime.UtcNow;
    }
}
