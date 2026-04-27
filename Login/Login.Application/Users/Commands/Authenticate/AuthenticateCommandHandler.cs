using Login.Application.Common.Interfaces;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.Authenticate;

public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, AuthenticateResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ICryptography _cryptography;
    private readonly ITokenManager _tokenManager;
    private readonly IModuleRepository _moduleRepository;

    public AuthenticateCommandHandler(
        IUserRepository userRepository,
        ICryptography cryptography,
        ITokenManager tokenManager,
        IModuleRepository moduleRepository)
    {
        _userRepository = userRepository;
        _cryptography = cryptography;
        _tokenManager = tokenManager;
        _moduleRepository = moduleRepository;
    }

    public async Task<AuthenticateResult> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!user.IsActive || user.IsBlocked)
            throw new UnauthorizedAccessException("Usuário inativo ou bloqueado.");

        if (!_cryptography.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var token = _tokenManager.Generate(user);

        var modules = await _moduleRepository.GetByProfileAsync(user.ProfileId ?? Guid.Empty, cancellationToken);


        var restrictions = user.Restrictions
            .Select(r => new RestrictionDto(r.ModuleId, r.CompanyId))
            .ToList();

        return new AuthenticateResult(
            AccessToken: token,
            AvatarUrl: user.AvatarUrl,
            Hierarchies: new List<HierarchyDto>(),
            Restrictions: restrictions,
            SelectedCompanies: new List<int>());
    }
}
