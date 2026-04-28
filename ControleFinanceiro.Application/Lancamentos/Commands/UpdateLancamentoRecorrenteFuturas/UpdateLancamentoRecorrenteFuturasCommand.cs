using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamentoRecorrenteFuturas;

public record UpdateLancamentoRecorrenteFuturasCommand(
    Guid Id,
    string Descricao,
    DateTime Data,
    decimal Valor,
    TipoLancamento Tipo,
    SituacaoLancamento Situacao,
    Guid? CategoriaId,
    Guid? CartaoId = null
) : IRequest;
