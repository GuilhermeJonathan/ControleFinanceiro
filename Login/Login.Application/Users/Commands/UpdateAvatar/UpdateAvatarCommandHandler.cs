using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.UpdateAvatar;

public class UpdateAvatarCommandHandler : IRequestHandler<UpdateAvatarCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccessor _userAccessor;

    public UpdateAvatarCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserAccessor userAccessor)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userAccessor = userAccessor;
    }

    public async Task Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userAccessor.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        user.UpdateAvatar(request.AvatarUrl);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
