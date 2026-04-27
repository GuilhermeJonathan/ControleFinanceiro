using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    private readonly IInviteRepository _inviteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICryptography _cryptography;
    private readonly ITokenManager _tokenManager;

    public RegisterUserCommandHandler(
        IInviteRepository inviteRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICryptography cryptography,
        ITokenManager tokenManager)
    {
        _inviteRepository = inviteRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cryptography = cryptography;
        _tokenManager = tokenManager;
    }

    public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var invite = await _inviteRepository.GetByTokenAsync(request.InviteToken, cancellationToken)
            ?? throw new InvalidOperationException("Convite inválido ou não encontrado.");

        if (!invite.IsValid)
            throw new InvalidOperationException("Convite expirado ou já utilizado.");

        if (invite.Email is not null &&
            !string.Equals(invite.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Este convite está vinculado a outro e-mail.");

        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");

        var passwordHash = _cryptography.Hash(request.Password);

        var user = new User(
            id: Guid.NewGuid(),
            name: request.Name,
            email: request.Email,
            document: request.Document ?? string.Empty,
            passwordHash: passwordHash,
            userTypeId: UserType.User);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        invite.Use(request.Email);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _tokenManager.Generate(user);
    }
}
