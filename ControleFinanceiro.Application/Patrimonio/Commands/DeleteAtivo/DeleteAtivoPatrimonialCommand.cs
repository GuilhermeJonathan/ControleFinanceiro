using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.DeleteAtivo;

public record DeleteAtivoPatrimonialCommand(Guid Id) : IRequest;

public class DeleteAtivoPatrimonialCommandHandler(
    IAtivoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAtivoPatrimonialCommand>
{
    public async Task Handle(DeleteAtivoPatrimonialCommand request, CancellationToken cancellationToken)
    {
        var ativo = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Ativo {request.Id} não encontrado.");

        if (ativo.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao ativo.");

        repository.Remove(ativo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
