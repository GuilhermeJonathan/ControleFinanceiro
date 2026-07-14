using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.DeleteInvestimento;

public record DeleteInvestimentoCommand(Guid Id) : IRequest;

public class DeleteInvestimentoCommandHandler(
    IInvestimentoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteInvestimentoCommand>
{
    public async Task Handle(DeleteInvestimentoCommand request, CancellationToken cancellationToken)
    {
        var inv = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Investimento {request.Id} nao encontrado.");

        if (inv.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao investimento.");

        repository.Remove(inv);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
