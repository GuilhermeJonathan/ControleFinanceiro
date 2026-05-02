using FluentValidation;

namespace Login.Application.Terms.Commands.AcceptTerm;

public class AcceptTermCommandValidator : AbstractValidator<AcceptTermCommand>
{
    public AcceptTermCommandValidator()
    {
        RuleFor(x => x.TermName)
            .NotEmpty().WithMessage("Nome do termo é obrigatório.")
            .MaximumLength(100).WithMessage("Nome do termo deve ter no máximo 100 caracteres.");
    }
}
