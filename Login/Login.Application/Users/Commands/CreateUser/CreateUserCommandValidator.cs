using FluentValidation;

namespace Login.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Documento é obrigatório.")
            .Length(11, 14).WithMessage("CPF/CNPJ inválido.");

        RuleFor(x => x.UserTypeId)
            .InclusiveBetween(1, 4).WithMessage("Tipo de usuário inválido.");
    }
}
