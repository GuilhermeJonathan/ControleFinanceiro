using Login.Application.Common.Interfaces;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Terms.Queries.CheckTermAccepted;

public class CheckTermAcceptedQueryHandler : IRequestHandler<CheckTermAcceptedQuery, bool>
{
    private readonly ITermRepository _termRepository;
    private readonly IUserAccessor _userAccessor;

    public CheckTermAcceptedQueryHandler(ITermRepository termRepository, IUserAccessor userAccessor)
    {
        _termRepository = termRepository;
        _userAccessor = userAccessor;
    }

    public async Task<bool> Handle(CheckTermAcceptedQuery request, CancellationToken cancellationToken)
        => await _termRepository.HasAcceptedAsync(_userAccessor.UserId, request.TermName, cancellationToken);
}
