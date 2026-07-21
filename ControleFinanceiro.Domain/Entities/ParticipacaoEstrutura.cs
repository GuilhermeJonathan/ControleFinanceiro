using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Aresta do grafo de estruturas: quem detém quem.
/// EstruturaPaiId == null significa que o detentor é a FAMÍLIA/cliente (topo do grafo).
/// </summary>
public class ParticipacaoEstrutura
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    /// <summary>Estrutura detentora. null = família/cliente (raiz do grafo).</summary>
    public Guid? EstruturaPaiId { get; private set; }
    /// <summary>Estrutura detida.</summary>
    public Guid EstruturaFilhaId { get; private set; }
    /// <summary>Percentual de participação (0–100).</summary>
    public decimal PercentualParticipacao { get; private set; }
    public TipoRelacaoEstrutura TipoRelacao { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private ParticipacaoEstrutura() { }

    public ParticipacaoEstrutura(Guid usuarioId, Guid? estruturaPaiId, Guid estruturaFilhaId,
        decimal percentualParticipacao, TipoRelacaoEstrutura tipoRelacao)
    {
        UsuarioId = usuarioId;
        EstruturaPaiId = estruturaPaiId;
        EstruturaFilhaId = estruturaFilhaId;
        PercentualParticipacao = percentualParticipacao;
        TipoRelacao = tipoRelacao;
    }

    public void Atualizar(decimal percentualParticipacao, TipoRelacaoEstrutura tipoRelacao)
    {
        PercentualParticipacao = percentualParticipacao;
        TipoRelacao = tipoRelacao;
    }
}
