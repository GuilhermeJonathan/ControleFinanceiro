using FluentValidation;

namespace ControleFinanceiro.Application.Cartoes.Commands.DeleteParcela;

public class DeleteParcelaCommandValidator : AbstractValidator<DeleteParcelaCommand>
{
    public DeleteParcelaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
