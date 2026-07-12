using FluentValidation;

namespace Login.Application.Payments.Commands.CreateCheckout;

public class CreateCheckoutCommandValidator : AbstractValidator<CreateCheckoutCommand>
{
    private static readonly string[] ValidPlans = ["mensal", "anual", "assessor"];

    public CreateCheckoutCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .Must(p => ValidPlans.Contains(p.ToLower()))
            .WithMessage("PlanId deve ser 'mensal', 'anual' ou 'assessor'.");
    }
}
