using FluentValidation;

namespace ControleFinanceiro.Application.SaldoContas.Commands.UpsertSaldo;

public class UpsertSaldoCommandValidator : AbstractValidator<UpsertSaldoCommand>
{
    public UpsertSaldoCommandValidator()
    {
        RuleFor(x => x.Banco)
            .NotEmpty().WithMessage("Banco é obrigatório.")
            .MaximumLength(100).WithMessage("Banco deve ter no máximo 100 caracteres.");
    }
}
