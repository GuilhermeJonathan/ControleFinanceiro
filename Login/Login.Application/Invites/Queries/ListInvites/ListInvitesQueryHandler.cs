using Login.Application.Common.Interfaces;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Invites.Queries.ListInvites;

public class ListInvitesQueryHandler : IRequestHandler<ListInvitesQuery, IReadOnlyList<InviteDto>>
{
    private readonly IInviteRepository _inviteRepository;
    private readonly IUserAccessor _userAccessor;

    public ListInvitesQueryHandler(IInviteRepository inviteRepository, IUserAccessor userAccessor)
    {
        _inviteRepository = inviteRepository;
        _userAccessor = userAccessor;
    }

    public async Task<IReadOnlyList<InviteDto>> Handle(ListInvitesQuery request, CancellationToken cancellationToken)
    {
        var invites = await _inviteRepository.GetByUserAsync(_userAccessor.UserId, cancellationToken);

        return invites
            .Select(i => new InviteDto(i.Token, i.Email, i.ExpiresAt, i.IsValid, i.UsedAt))
            .ToList();
    }
}
