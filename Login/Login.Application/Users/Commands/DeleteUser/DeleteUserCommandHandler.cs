using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenManager _tokenManager;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenManager tokenManager)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenManager = tokenManager;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {request.Id} não encontrado.");

        _tokenManager.Invalidate(user.Id);
        _userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
