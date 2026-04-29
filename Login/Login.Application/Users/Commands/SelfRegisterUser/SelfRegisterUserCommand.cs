using Login.Application.Users.Commands.Authenticate;
using MediatR;

namespace Login.Application.Users.Commands.SelfRegisterUser;

/// <summary>Auto-cadastro público — sem convite.</summary>
public record SelfRegisterUserCommand(
    string Name,
    string Email,
    string Password,
    string? Document
) : IRequest<SelfRegisterResult>;

public record SelfRegisterResult(
    string AccessToken,
    string? AvatarUrl,
    PlanInfoDto PlanInfo
);
