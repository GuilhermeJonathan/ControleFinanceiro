using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Queries.GetReceitasRecorrentes;

public record ReceitaRecorrenteDto(
    Guid Id,
    string Nome,
    TipoReceita Tipo,
    decimal Valor,
    decimal? ValorHora,
    decimal? QuantidadeHoras,
    int Dia,
    DateTime DataInicio
);
