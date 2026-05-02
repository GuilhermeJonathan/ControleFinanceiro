using FluentValidation;

namespace ControleFinanceiro.Application.Horas.Commands.UpdateHoras;

public class UpdateHorasCommandValidator : AbstractValidator<UpdateHorasCommand>
{
    public UpdateHorasCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ValorHora)
            .GreaterThan(0).WithMessage("Valor da hora deve ser maior que zero.");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
    }
}
