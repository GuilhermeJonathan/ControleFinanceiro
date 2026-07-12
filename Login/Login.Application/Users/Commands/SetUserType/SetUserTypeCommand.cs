using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.SetUserType;

public record SetUserTypeCommand(Guid Id, int UserTypeId) : IRequest;

public class SetUserTypeCommandHandler : IRequestHandler<SetUserTypeCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserTypeCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetUserTypeCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(UserType), request.UserTypeId))
            throw new ArgumentException($"Tipo de usuário inválido: {request.UserTypeId}.");

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {request.Id} não encontrado.");

        user.SetUserType((UserType)request.UserTypeId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
