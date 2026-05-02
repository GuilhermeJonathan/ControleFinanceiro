using FluentValidation;

namespace ControleFinanceiro.Application.SaldoContas.Commands.UpdateConta;

public class UpdateContaCommandValidator : AbstractValidator<UpdateContaCommand>
{
    public UpdateContaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Banco)
            .NotEmpty().WithMessage("Banco é obrigatório.")
            .MaximumLength(100).WithMessage("Banco deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de conta inválido.");
    }
}
