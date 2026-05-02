using FluentValidation;

namespace ControleFinanceiro.Application.Metas.Commands.AtualizarValorMeta;

public class AtualizarValorMetaCommandValidator : AbstractValidator<AtualizarValorMetaCommand>
{
    public AtualizarValorMetaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.NovoValor)
            .GreaterThan(0).WithMessage("Novo valor deve ser maior que zero.");
    }
}
