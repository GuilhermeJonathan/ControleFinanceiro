using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.SetPodeVerImoveis;

public record SetPodeVerImoveisCommand(Guid Id, bool Value) : IRequest;

public class SetPodeVerImoveisCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetPodeVerImoveisCommand>
{
    public async Task Handle(SetPodeVerImoveisCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {request.Id} não encontrado.");

        user.SetPodeVerImoveis(request.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
