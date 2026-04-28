namespace ControleFinanceiro.Application.Lancamentos.Queries.GetProjecao;

public record ProjecaoMesDto(
    int Mes,
    int Ano,
    string Label,
    decimal TotalCreditos,
    decimal TotalDebitos);
