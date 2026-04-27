using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Invites.Commands.CreateInvite;

public class CreateInviteCommandHandler : IRequestHandler<CreateInviteCommand, CreateInviteResult>
{
    private readonly IInviteRepository _inviteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccessor _userAccessor;

    public CreateInviteCommandHandler(
        IInviteRepository inviteRepository,
        IUnitOfWork unitOfWork,
        IUserAccessor userAccessor)
    {
        _inviteRepository = inviteRepository;
        _unitOfWork = unitOfWork;
        _userAccessor = userAccessor;
    }

    public async Task<CreateInviteResult> Handle(CreateInviteCommand request, CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays);

        var invite = new Invite(
            id: Guid.NewGuid(),
            token: token,
            expiresAt: expiresAt,
            createdByUserId: _userAccessor.UserId,
            email: request.Email);

        await _inviteRepository.AddAsync(invite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateInviteResult(token, expiresAt);
    }
}
