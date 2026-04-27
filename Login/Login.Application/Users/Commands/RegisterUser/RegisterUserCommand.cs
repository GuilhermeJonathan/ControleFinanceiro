using MediatR;

namespace Login.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string InviteToken,
    string Name,
    string Email,
    string Password,
    string? Document
) : IRequest<string>;
