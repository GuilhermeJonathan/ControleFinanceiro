using FluentValidation;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteParcelasFuturas;

public class DeleteParcelasFuturasCommandValidator : AbstractValidator<DeleteParcelasFuturasCommand>
{
    public DeleteParcelasFuturasCommandValidator()
    {
        RuleFor(x => x.GrupoParcelas)
            .NotEmpty().WithMessage("GrupoParcelas é obrigatório.");

        RuleFor(x => x.ParcelaAtualFrom)
            .GreaterThan(0).WithMessage("ParcelaAtualFrom deve ser maior que zero.");
    }
}
