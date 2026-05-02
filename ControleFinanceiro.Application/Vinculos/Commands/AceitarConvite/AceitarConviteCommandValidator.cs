using FluentValidation;

namespace ControleFinanceiro.Application.Vinculos.Commands.AceitarConvite;

public class AceitarConviteCommandValidator : AbstractValidator<AceitarConviteCommand>
{
    public AceitarConviteCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Código do convite é obrigatório.")
            .MaximumLength(20).WithMessage("Código do convite deve ter no máximo 20 caracteres.");

        RuleFor(x => x.NomeMembro)
            .NotEmpty().WithMessage("Nome do membro é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");
    }
}
