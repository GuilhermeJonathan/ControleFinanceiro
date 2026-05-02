using FluentValidation;

namespace ControleFinanceiro.Application.Categorias.Commands.AtualizarLimiteCategoria;

public class AtualizarLimiteCategoriaCommandValidator : AbstractValidator<AtualizarLimiteCategoriaCommand>
{
    public AtualizarLimiteCategoriaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.LimiteMensal)
            .GreaterThan(0).WithMessage("Limite mensal deve ser maior que zero.")
            .When(x => x.LimiteMensal.HasValue);
    }
}
