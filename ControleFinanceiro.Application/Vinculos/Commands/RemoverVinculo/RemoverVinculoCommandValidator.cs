using FluentValidation;

namespace ControleFinanceiro.Application.Vinculos.Commands.RemoverVinculo;

public class RemoverVinculoCommandValidator : AbstractValidator<RemoverVinculoCommand>
{
    public RemoverVinculoCommandValidator()
    {
        RuleFor(x => x.VinculoId)
            .NotEmpty().WithMessage("VinculoId é obrigatório.");
    }
}
