using FluentValidation;

namespace Login.Application.Users.Commands.ResetPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Identificador)
            .NotEmpty().WithMessage("Identificador é obrigatório.")
            .MaximumLength(200).WithMessage("Identificador deve ter no máximo 200 caracteres.");
    }
}

public class ValidateHashCommandValidator : AbstractValidator<ValidateHashCommand>
{
    public ValidateHashCommandValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Documento é obrigatório.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");
    }
}

public class ValidateSecurityCodeCommandValidator : AbstractValidator<ValidateSecurityCodeCommand>
{
    public ValidateSecurityCodeCommandValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Documento é obrigatório.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");
    }
}

public class RedefinePasswordCommandValidator : AbstractValidator<RedefinePasswordCommand>
{
    public RedefinePasswordCommandValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Documento é obrigatório.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");
    }
}
