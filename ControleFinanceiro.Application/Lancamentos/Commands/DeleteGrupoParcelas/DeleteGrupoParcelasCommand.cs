using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteGrupoParcelas;

public record DeleteGrupoParcelasCommand(Guid GrupoParcelas) : IRequest;
