using FluentValidation;

namespace Login.Application.Profiles.Commands.CreateProfile;

public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.UserTypeId)
            .InclusiveBetween(1, 4).WithMessage("Tipo de usuário inválido.");
    }
}
