using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.UpdateSelf;

public class UpdateSelfCommandHandler : IRequestHandler<UpdateSelfCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccessor _userAccessor;

    public UpdateSelfCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserAccessor userAccessor)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userAccessor = userAccessor;
    }

    public async Task Handle(UpdateSelfCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userAccessor.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        user.UpdateProfile(request.Name, null, null, request.Cellphone, null, null, null);
        user.UpdateDocument(request.Document);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
