using FluentValidation;

namespace ControleFinanceiro.Application.SaldoContas.Commands.CreateConta;

public class CreateContaCommandValidator : AbstractValidator<CreateContaCommand>
{
    public CreateContaCommandValidator()
    {
        RuleFor(x => x.Banco)
            .NotEmpty().WithMessage("Banco é obrigatório.")
            .MaximumLength(100).WithMessage("Banco deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de conta inválido.");
    }
}
