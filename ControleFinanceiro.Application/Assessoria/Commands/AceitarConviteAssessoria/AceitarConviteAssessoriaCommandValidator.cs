using FluentValidation;

namespace ControleFinanceiro.Application.Assessoria.Commands.AceitarConviteAssessoria;

public class AceitarConviteAssessoriaCommandValidator : AbstractValidator<AceitarConviteAssessoriaCommand>
{
    public AceitarConviteAssessoriaCommandValidator()
    {
        RuleFor(c => c.Codigo)
            .NotEmpty().WithMessage("Código do convite é obrigatório.")
            .Length(6).WithMessage("Código do convite deve ter 6 caracteres.");

        RuleFor(c => c.NomeCliente)
            .NotEmpty().WithMessage("Nome do cliente é obrigatório.")
            .MaximumLength(200);
    }
}
