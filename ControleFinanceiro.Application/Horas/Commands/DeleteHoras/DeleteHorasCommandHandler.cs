using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Horas.Commands.DeleteHoras;

public class DeleteHorasCommandHandler(IHorasTrabalhadasRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteHorasCommand>
{
    public async Task Handle(DeleteHorasCommand request, CancellationToken cancellationToken)
    {
        var horas = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Registro de horas {request.Id} não encontrado.");

        repository.Delete(horas);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
