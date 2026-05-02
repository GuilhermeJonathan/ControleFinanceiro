using FluentValidation;

namespace ControleFinanceiro.Application.Cartoes.Commands.CreateCartao;

public class CreateCartaoCommandValidator : AbstractValidator<CreateCartaoCommand>
{
    public CreateCartaoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.DiaVencimento)
            .InclusiveBetween(1, 31).WithMessage("Dia de vencimento deve estar entre 1 e 31.")
            .When(x => x.DiaVencimento.HasValue);
    }
}
