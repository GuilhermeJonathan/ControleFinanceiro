using FluentValidation;

namespace Login.Application.Invites.Commands.CreateInvite;

public class CreateInviteCommandValidator : AbstractValidator<CreateInviteCommand>
{
    public CreateInviteCommandValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(200).WithMessage("E-mail deve ter no máximo 200 caracteres.")
            .When(x => x.Email != null);

        RuleFor(x => x.ExpirationDays)
            .GreaterThan(0).WithMessage("Dias de expiração deve ser maior que zero.");
    }
}
