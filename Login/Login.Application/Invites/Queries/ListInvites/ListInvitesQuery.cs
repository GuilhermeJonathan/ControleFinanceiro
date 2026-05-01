using MediatR;

namespace Login.Application.Invites.Queries.ListInvites;

public record ListInvitesQuery : IRequest<IReadOnlyList<InviteDto>>;

public record InviteDto(string Token, string? Email, DateTime ExpiresAt, bool IsValid, DateTime? UsedAt);
