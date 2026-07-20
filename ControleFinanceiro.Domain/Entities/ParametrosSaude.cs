namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Parâmetros do termômetro de saúde financeira, por assessor (consultoria).
/// Quando não há registro, usa <see cref="Padrao"/> (mesmos limites do código original).
/// </summary>
public class ParametrosSaude
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AssessorId { get; private set; }

    // Faixas de classificação (score 0-100)
    public int ScoreExcelenteMin { get; private set; } = 80;
    public int ScoreBoaMin { get; private set; } = 60;
    public int ScoreAtencaoMin { get; private set; } = 40;

    // Comprometimento de renda (% das despesas sobre a renda) — quanto menor, melhor
    public int ComprometimentoSaudavelMax { get; private set; } = 50;
    public int ComprometimentoRazoavelMax { get; private set; } = 70;
    public int ComprometimentoApertadoMax { get; private set; } = 85;

    // Reserva (dias de gasto cobertos) — quanto maior, melhor
    public int ReservaExcelenteMinDias { get; private set; } = 90;
    public int ReservaBoaMinDias { get; private set; } = 30;
    public int ReservaCurtaMinDias { get; private set; } = 15;

    private ParametrosSaude() { }

    public ParametrosSaude(Guid assessorId) { AssessorId = assessorId; }

    /// <summary>Instância transitória com os valores padrão (assessor sem config própria).</summary>
    public static ParametrosSaude Padrao() => new();

    public void Atualizar(
        int scoreExcelenteMin, int scoreBoaMin, int scoreAtencaoMin,
        int comprometimentoSaudavelMax, int comprometimentoRazoavelMax, int comprometimentoApertadoMax,
        int reservaExcelenteMinDias, int reservaBoaMinDias, int reservaCurtaMinDias)
    {
        // Ordena para evitar limites incoerentes (cortes decrescentes / crescentes).
        ScoreExcelenteMin = scoreExcelenteMin;
        ScoreBoaMin = scoreBoaMin;
        ScoreAtencaoMin = scoreAtencaoMin;
        ComprometimentoSaudavelMax = comprometimentoSaudavelMax;
        ComprometimentoRazoavelMax = comprometimentoRazoavelMax;
        ComprometimentoApertadoMax = comprometimentoApertadoMax;
        ReservaExcelenteMinDias = reservaExcelenteMinDias;
        ReservaBoaMinDias = reservaBoaMinDias;
        ReservaCurtaMinDias = reservaCurtaMinDias;
    }
}
