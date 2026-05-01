using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.SetPlan;

public class SetPlanCommandHandler : IRequestHandler<SetPlanCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPlanCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetPlanCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {request.Id} não encontrado.");

        var planType = (PlanType)request.PlanType;

        switch (planType)
        {
            case PlanType.None:
                user.AdminClearPlan();
                break;

            case PlanType.Trial:
                var days = request.TrialDays ?? 30;
                user.AdminSetTrial(days);
                break;

            case PlanType.Monthly:
                user.SetPlan(PlanType.Monthly, DateTime.UtcNow.AddDays(30));
                break;

            case PlanType.Annual:
                user.SetPlan(PlanType.Annual, DateTime.UtcNow.AddDays(365));
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
