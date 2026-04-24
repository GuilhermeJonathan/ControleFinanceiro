using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamento;

public record UpdateLancamentoCommand(
    Guid Id,
    string Descricao,
    DateTime Data,
    decimal Valor,
    TipoLancamento Tipo,
    SituacaoLancamento Situacao,
    Guid? CategoriaId,
    Guid? CartaoId = null
) : IRequest;
