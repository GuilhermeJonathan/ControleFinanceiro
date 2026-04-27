using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Horas.Commands.CreateHoras;

public class CreateHorasCommandHandler(
    IHorasTrabalhadasRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateHorasCommand, Guid>
{
    public async Task<Guid> Handle(CreateHorasCommand request, CancellationToken cancellationToken)
    {
        var horas = new HorasTrabalhadas(request.Descricao, request.ValorHora, request.Quantidade, request.Mes, request.Ano, currentUser.UserId);
        await repository.AddAsync(horas, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return horas.Id;
    }
}
