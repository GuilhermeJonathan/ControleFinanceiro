using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.AtualizarSituacao;

public record AtualizarSituacaoCommand(Guid Id, SituacaoLancamento Situacao, Guid? ContaBancariaId = null) : IRequest;
