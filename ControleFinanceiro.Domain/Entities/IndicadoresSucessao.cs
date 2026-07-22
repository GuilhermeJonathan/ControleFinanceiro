namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Indicadores de governança/conformidade da sucessão, informados pelo assessor por cliente
/// (0–100). O planejamento sucessório NÃO fica aqui: é derivado do progresso do plano de ação.
/// Uma linha por usuário (cliente).
/// </summary>
public class IndicadoresSucessao
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public int? GovernancaScore { get; private set; }
    public int? ConformidadeScore { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private IndicadoresSucessao() { }

    public IndicadoresSucessao(Guid usuarioId, int? governanca, int? conformidade)
    {
        UsuarioId = usuarioId;
        Atualizar(governanca, conformidade);
    }

    public void Atualizar(int? governanca, int? conformidade)
    {
        GovernancaScore = Clamp(governanca);
        ConformidadeScore = Clamp(conformidade);
        AtualizadoEm = DateTime.UtcNow;
    }

    private static int? Clamp(int? v) => v is null ? null : Math.Clamp(v.Value, 0, 100);
}
