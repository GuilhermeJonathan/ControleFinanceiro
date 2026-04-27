using MediatR;

namespace Login.Application.Invites.Commands.CreateInvite;

public record CreateInviteCommand(string? Email, int ExpirationDays = 7) : IRequest<CreateInviteResult>;

public record CreateInviteResult(string Token, DateTime ExpiresAt);
