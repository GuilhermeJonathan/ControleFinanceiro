namespace ControleFinanceiro.Domain.Enums;

public enum SituacaoLancamento
{
    Recebido = 1,
    Pago = 2,
    AReceber = 3,
    AVencer = 4,
    Vencido = 5,
    /// <summary>
    /// Parcela recorrente excluída pelo usuário. Mantida no banco para que o
    /// DailyJobService inclua o registro no cálculo do horizonte (MAX AnoMes) e
    /// não regenere automaticamente o mês cancelado. Filtrada de todas as
    /// queries de exibição.
    /// </summary>
    Cancelado = 6
}
