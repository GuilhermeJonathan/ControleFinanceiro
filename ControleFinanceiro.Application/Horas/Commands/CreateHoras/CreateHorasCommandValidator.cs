using FluentValidation;

namespace ControleFinanceiro.Application.Horas.Commands.CreateHoras;

public class CreateHorasCommandValidator : AbstractValidator<CreateHorasCommand>
{
    public CreateHorasCommandValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ValorHora)
            .GreaterThan(0).WithMessage("Valor da hora deve ser maior que zero.");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");

        RuleFor(x => x.Mes)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12.");

        RuleFor(x => x.Ano)
            .GreaterThan(2000).WithMessage("Ano inválido.");
    }
}
