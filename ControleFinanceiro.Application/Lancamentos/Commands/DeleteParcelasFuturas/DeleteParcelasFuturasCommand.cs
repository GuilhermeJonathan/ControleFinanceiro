using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteParcelasFuturas;

/// <summary>
/// Exclui todas as parcelas futuras de um grupo a partir da parcela informada (inclusive).
/// </summary>
public record DeleteParcelasFuturasCommand(Guid GrupoParcelas, int ParcelaAtualFrom) : IRequest;
