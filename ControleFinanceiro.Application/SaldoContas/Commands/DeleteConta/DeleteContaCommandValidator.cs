using FluentValidation;

namespace ControleFinanceiro.Application.SaldoContas.Commands.DeleteConta;

public class DeleteContaCommandValidator : AbstractValidator<DeleteContaCommand>
{
    public DeleteContaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
