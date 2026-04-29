using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.Authenticate;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.SelfRegisterUser;

public class SelfRegisterUserCommandHandler : IRequestHandler<SelfRegisterUserCommand, SelfRegisterResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICryptography _cryptography;
    private readonly ITokenManager _tokenManager;

    public SelfRegisterUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICryptography cryptography,
        ITokenManager tokenManager)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cryptography = cryptography;
        _tokenManager = tokenManager;
    }

    public async Task<SelfRegisterResult> Handle(SelfRegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Já existe uma conta com este e-mail.");

        var passwordHash = _cryptography.Hash(request.Password);

        var user = new User(
            id: Guid.NewGuid(),
            name: request.Name,
            email: request.Email,
            document: request.Document ?? string.Empty,
            passwordHash: passwordHash,
            userTypeId: UserType.User);

        // Inicia trial de 30 dias
        user.StartTrial();

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _tokenManager.Generate(user);
        var plan = user.GetPlanStatus();

        return new SelfRegisterResult(
            AccessToken: token,
            AvatarUrl: null,
            PlanInfo: new PlanInfoDto(
                HasPaidPlan: plan.HasPaidPlan,
                IsTrialActive: plan.IsTrialActive,
                IsTrialExpired: plan.IsTrialExpired,
                TrialDaysRemaining: plan.TrialDaysRemaining,
                TrialEndsAt: plan.TrialEndsAt,
                PlanExpiresAt: plan.PlanExpiresAt));
    }
}
