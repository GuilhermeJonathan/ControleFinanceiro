using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.DeleteAtivo;

public record DeleteAtivoPatrimonialCommand(Guid Id) : IRequest;

public class DeleteAtivoPatrimonialCommandHandler(
    IAtivoPatrimonialRepository repository,
    IPassivoPatrimonialRepository passivoRepository,
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

        // Solta dívidas atreladas a este ativo (evita vínculo órfão).
        var passivos = await passivoRepository.GetByUsuarioAsync(currentUser.UserId, cancellationToken);
        foreach (var p in passivos.Where(p => p.AtivoVinculadoId == ativo.Id))
        {
            p.DesvincularAtivo();
            passivoRepository.Update(p);
        }

        repository.Remove(ativo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
