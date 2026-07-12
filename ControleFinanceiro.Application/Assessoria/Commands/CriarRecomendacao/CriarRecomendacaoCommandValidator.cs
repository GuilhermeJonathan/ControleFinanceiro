using FluentValidation;

namespace ControleFinanceiro.Application.Assessoria.Commands.CriarRecomendacao;

public class CriarRecomendacaoCommandValidator : AbstractValidator<CriarRecomendacaoCommand>
{
    public CriarRecomendacaoCommandValidator()
    {
        RuleFor(c => c.ClienteId).NotEmpty();
        RuleFor(c => c.Texto)
            .NotEmpty().WithMessage("Texto da recomendação é obrigatório.")
            .MaximumLength(2000);
        RuleFor(c => c.Tipo).InclusiveBetween(1, 3);
    }
}
