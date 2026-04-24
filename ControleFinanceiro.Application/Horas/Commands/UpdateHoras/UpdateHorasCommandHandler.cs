using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Horas.Commands.UpdateHoras;

public class UpdateHorasCommandHandler(IHorasTrabalhadasRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateHorasCommand>
{
    public async Task Handle(UpdateHorasCommand request, CancellationToken cancellationToken)
    {
        var horas = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Registro de horas {request.Id} não encontrado.");

        horas.Update(request.Descricao, request.ValorHora, request.Quantidade);
        repository.Update(horas);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
