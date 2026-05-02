using FluentValidation;

namespace Login.Application.Integration.Commands.Authorize;

public class AuthorizeIntegrationCommandValidator : AbstractValidator<AuthorizeIntegrationCommand>
{
    public AuthorizeIntegrationCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId é obrigatório.")
            .MaximumLength(100).WithMessage("ClientId deve ter no máximo 100 caracteres.");

        RuleFor(x => x.ClientSecret)
            .NotEmpty().WithMessage("ClientSecret é obrigatório.")
            .MaximumLength(200).WithMessage("ClientSecret deve ter no máximo 200 caracteres.");
    }
}
