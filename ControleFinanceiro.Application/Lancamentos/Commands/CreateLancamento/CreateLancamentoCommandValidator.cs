using FluentValidation;

namespace ControleFinanceiro.Application.Lancamentos.Commands.CreateLancamento;

public class CreateLancamentoCommandValidator : AbstractValidator<CreateLancamentoCommand>
{
    public CreateLancamentoCommandValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.Mes).InclusiveBetween(1, 12);
        RuleFor(x => x.Ano).GreaterThan(2000);
    }
}
