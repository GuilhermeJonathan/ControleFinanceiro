using MediatR;

namespace Login.Application.Invites.Queries.ValidateInvite;

public record ValidateInviteQuery(string Token) : IRequest<ValidateInviteResult>;

public record ValidateInviteResult(bool IsValid, string? Email, DateTime? ExpiresAt);
