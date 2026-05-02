using FluentValidation;

namespace ControleFinanceiro.Application.Cartoes.Commands.UpdateParcela;

public class UpdateParcelaCommandValidator : AbstractValidator<UpdateParcelaCommand>
{
    public UpdateParcelaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ValorParcela)
            .GreaterThan(0).WithMessage("Valor da parcela deve ser maior que zero.");

        RuleFor(x => x.ParcelaAtual)
            .GreaterThan(0).WithMessage("Parcela atual deve ser maior que zero.");

        RuleFor(x => x.TotalParcelas)
            .GreaterThan(0).WithMessage("Total de parcelas deve ser maior que zero.");

        RuleFor(x => x.ParcelaAtual)
            .LessThanOrEqualTo(x => x.TotalParcelas)
            .WithMessage("Parcela atual não pode ser maior que o total de parcelas.");
    }
}
