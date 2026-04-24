using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.UpdateReceitaRecorrente;

public record UpdateReceitaRecorrenteCommand(
    Guid Id,
    string Nome,
    TipoReceita Tipo,
    int Dia,
    bool AplicarFuturos = false,
    decimal? Valor = null,
    decimal? ValorHora = null,
    decimal? QuantidadeHoras = null
) : IRequest;
