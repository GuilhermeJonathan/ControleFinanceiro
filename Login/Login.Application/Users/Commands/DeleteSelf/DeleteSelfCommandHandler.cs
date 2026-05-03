using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.DeleteSelf;

public class DeleteSelfCommandHandler : IRequestHandler<DeleteSelfCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccessor _userAccessor;
    private readonly ITokenManager _tokenManager;

    public DeleteSelfCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserAccessor userAccessor,
        ITokenManager tokenManager)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userAccessor = userAccessor;
        _tokenManager = tokenManager;
    }

    public async Task Handle(DeleteSelfCommand request, CancellationToken cancellationToken)
    {
        var userId = _userAccessor.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        _tokenManager.Invalidate(user.Id);
        _userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
