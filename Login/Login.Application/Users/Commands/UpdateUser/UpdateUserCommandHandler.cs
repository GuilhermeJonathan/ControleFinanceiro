using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenManager _tokenManager;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenManager tokenManager)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenManager = tokenManager;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {request.Id} não encontrado.");

        user.UpdateProfile(
            request.Name,
            request.Occupation,
            request.Address,
            request.Cellphone,
            request.Phone,
            request.CountryId,
            request.Region);

        _userRepository.Update(user);
        _tokenManager.Invalidate(user.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
