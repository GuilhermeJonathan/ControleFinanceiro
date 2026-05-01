using MediatR;

namespace Login.Application.Users.Commands.SetPlan;

public record SetPlanCommand(Guid Id, int PlanType, int? TrialDays) : IRequest;
