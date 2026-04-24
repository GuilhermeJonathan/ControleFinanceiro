using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.CreateLancamento;

public record CreateLancamentoCommand(
    string Descricao,
    DateTime Data,
    decimal Valor,
    TipoLancamento Tipo,
    SituacaoLancamento Situacao,
    int Mes,
    int Ano,
    Guid? CategoriaId,
    Guid? CartaoId = null,
    int TotalParcelas = 1,
    bool IsRecorrente = false
) : IRequest<Guid>;
