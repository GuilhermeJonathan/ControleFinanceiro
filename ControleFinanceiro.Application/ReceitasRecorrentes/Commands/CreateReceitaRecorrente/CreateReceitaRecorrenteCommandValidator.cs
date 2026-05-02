using FluentValidation;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.CreateReceitaRecorrente;

public class CreateReceitaRecorrenteCommandValidator : AbstractValidator<CreateReceitaRecorrenteCommand>
{
    public CreateReceitaRecorrenteCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de receita inválido.");

        RuleFor(x => x.Dia)
            .InclusiveBetween(1, 31).WithMessage("Dia deve estar entre 1 e 31.");

        RuleFor(x => x.Meses)
            .GreaterThan(0).WithMessage("Meses deve ser maior que zero.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.")
            .When(x => x.Valor.HasValue);

        RuleFor(x => x.ValorHora)
            .GreaterThan(0).WithMessage("Valor por hora deve ser maior que zero.")
            .When(x => x.ValorHora.HasValue);

        RuleFor(x => x.QuantidadeHoras)
            .GreaterThan(0).WithMessage("Quantidade de horas deve ser maior que zero.")
            .When(x => x.QuantidadeHoras.HasValue);
    }
}
