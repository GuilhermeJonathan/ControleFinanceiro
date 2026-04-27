using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Invites.Queries.ValidateInvite;

public class ValidateInviteQueryHandler : IRequestHandler<ValidateInviteQuery, ValidateInviteResult>
{
    private readonly IInviteRepository _inviteRepository;

    public ValidateInviteQueryHandler(IInviteRepository inviteRepository)
    {
        _inviteRepository = inviteRepository;
    }

    public async Task<ValidateInviteResult> Handle(ValidateInviteQuery request, CancellationToken cancellationToken)
    {
        var invite = await _inviteRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (invite is null)
            return new ValidateInviteResult(false, null, null);

        return new ValidateInviteResult(invite.IsValid, invite.Email, invite.ExpiresAt);
    }
}
