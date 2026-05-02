using FluentValidation;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteLancamento;

public class DeleteLancamentoCommandValidator : AbstractValidator<DeleteLancamentoCommand>
{
    public DeleteLancamentoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
