using FluentValidation;

namespace ControleFinanceiro.Application.Cartoes.Commands.DeleteCartao;

public class DeleteCartaoCommandValidator : AbstractValidator<DeleteCartaoCommand>
{
    public DeleteCartaoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
