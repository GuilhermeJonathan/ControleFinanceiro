using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Alocação-alvo (% desejado) de uma classe de investimento para um usuário.
/// Comparada com a alocação real para orientar rebalanceamento.
/// </summary>
public class AlocacaoAlvo
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public TipoInvestimento Tipo { get; private set; }
    public decimal PercentualAlvo { get; private set; }

    private AlocacaoAlvo() { }

    public static AlocacaoAlvo Criar(Guid usuarioId, TipoInvestimento tipo, decimal percentualAlvo) =>
        new() { UsuarioId = usuarioId, Tipo = tipo, PercentualAlvo = Math.Clamp(percentualAlvo, 0m, 100m) };
}
