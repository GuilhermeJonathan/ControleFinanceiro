using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.DeletePassivo;

public record DeletePassivoPatrimonialCommand(Guid Id) : IRequest;

public class DeletePassivoPatrimonialCommandHandler(
    IPassivoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePassivoPatrimonialCommand>
{
    public async Task Handle(DeletePassivoPatrimonialCommand request, CancellationToken cancellationToken)
    {
        var passivo = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Passivo {request.Id} não encontrado.");

        if (passivo.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao passivo.");

        repository.Remove(passivo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
