using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICryptography _cryptography;
    private readonly IUserAccessor _userAccessor;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICryptography cryptography,
        IUserAccessor userAccessor)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cryptography = cryptography;
        _userAccessor = userAccessor;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userAccessor.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!await _cryptography.VerifyAsync(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        if (request.NewPassword.Length < 6)
            throw new ArgumentException("A nova senha deve ter pelo menos 6 caracteres.");

        user.ChangePassword(_cryptography.Hash(request.NewPassword));
        user.RevokeTokens(); // força novo login após troca de senha
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
