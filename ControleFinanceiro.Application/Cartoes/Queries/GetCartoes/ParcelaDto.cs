namespace ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;

public record ParcelaDto(
    Guid Id,
    string Descricao,
    decimal ValorParcela,
    int ParcelaAtual,
    int TotalParcelas,
    DateTime DataInicio
);
