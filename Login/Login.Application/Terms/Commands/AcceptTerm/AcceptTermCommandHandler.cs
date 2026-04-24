using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Terms.Commands.AcceptTerm;

public class AcceptTermCommandHandler : IRequestHandler<AcceptTermCommand>
{
    private readonly ITermRepository _termRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccessor _userAccessor;

    public AcceptTermCommandHandler(
        ITermRepository termRepository,
        IUnitOfWork unitOfWork,
        IUserAccessor userAccessor)
    {
        _termRepository = termRepository;
        _unitOfWork = unitOfWork;
        _userAccessor = userAccessor;
    }

    public async Task Handle(AcceptTermCommand request, CancellationToken cancellationToken)
    {
        var term = new AcceptedTerm(
            Guid.NewGuid(),
            _userAccessor.UserId,
            request.TermName,
            request.IpAddress,
            request.UserAgent);

        await _termRepository.AddAsync(term, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
