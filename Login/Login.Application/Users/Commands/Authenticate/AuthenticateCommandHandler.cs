using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.Authenticate;

public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, AuthenticateResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICryptography _cryptography;
    private readonly ITokenManager _tokenManager;
    private readonly IModuleRepository _moduleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticateCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ICryptography cryptography,
        ITokenManager tokenManager,
        IModuleRepository moduleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _cryptography = cryptography;
        _tokenManager = tokenManager;
        _moduleRepository = moduleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthenticateResult> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!user.IsActive || user.IsBlocked)
            throw new UnauthorizedAccessException("Usuário inativo ou bloqueado.");

        if (!_cryptography.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        user.RegisterLogin();
        _userRepository.Update(user);

        // Gera refresh token (válido por 30 dias)
        var refreshToken = new RefreshToken(
            user.Id,
            Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
            DateTime.UtcNow.AddDays(30));
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenManager.Generate(user);

        var restrictions = user.Restrictions
            .Select(r => new RestrictionDto(r.ModuleId, r.CompanyId))
            .ToList();

        var plan = user.GetPlanStatus();
        var planInfo = new PlanInfoDto(
            HasPaidPlan: plan.HasPaidPlan,
            IsTrialActive: plan.IsTrialActive,
            IsTrialExpired: plan.IsTrialExpired,
            TrialDaysRemaining: plan.TrialDaysRemaining,
            TrialEndsAt: plan.TrialEndsAt,
            PlanExpiresAt: plan.PlanExpiresAt);

        return new AuthenticateResult(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            AvatarUrl: user.AvatarUrl,
            Hierarchies: new List<HierarchyDto>(),
            Restrictions: restrictions,
            SelectedCompanies: new List<int>(),
            PlanInfo: planInfo);
    }
}
