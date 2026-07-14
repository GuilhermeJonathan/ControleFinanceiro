using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Simulacoes.Commands.DeleteSimulacao;

public record DeleteSimulacaoCommand(Guid Id) : IRequest;

public class DeleteSimulacaoCommandHandler(
    ISimulacaoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSimulacaoCommand>
{
    public async Task Handle(DeleteSimulacaoCommand request, CancellationToken cancellationToken)
    {
        var simulacao = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Simulação {request.Id} não encontrada.");

        if (simulacao.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado à simulação.");

        repository.Remove(simulacao);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
