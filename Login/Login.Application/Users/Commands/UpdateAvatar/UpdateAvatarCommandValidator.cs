using FluentValidation;

namespace Login.Application.Users.Commands.UpdateAvatar;

public class UpdateAvatarCommandValidator : AbstractValidator<UpdateAvatarCommand>
{
    public UpdateAvatarCommandValidator()
    {
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("URL do avatar deve ter no máximo 500 caracteres.")
            .When(x => x.AvatarUrl != null);
    }
}
