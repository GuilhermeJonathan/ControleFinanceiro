using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.CreateReceitaRecorrente;

public record CreateReceitaRecorrenteCommand(
    string Nome,
    TipoReceita Tipo,
    int Dia,
    int Meses = 12,
    decimal? Valor = null,
    decimal? ValorHora = null,
    decimal? QuantidadeHoras = null
) : IRequest<Guid>;
