using FluentValidation;

namespace Login.Application.Users.Commands.SetPlan;

public class SetPlanCommandValidator : AbstractValidator<SetPlanCommand>
{
    public SetPlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id do usuário é obrigatório.");

        RuleFor(x => x.PlanType)
            .GreaterThan(0).WithMessage("Tipo de plano inválido.");

        RuleFor(x => x.TrialDays)
            .GreaterThan(0).WithMessage("Dias de trial deve ser maior que zero.")
            .When(x => x.TrialDays.HasValue);
    }
}
