namespace ControleFinanceiro.Application.Horas.Queries.GetHorasByMes;

public record HorasDto(Guid Id, string Descricao, decimal ValorHora, decimal Quantidade, decimal ValorTotal, int Mes, int Ano);
