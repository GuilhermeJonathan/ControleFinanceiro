using FluentValidation;

namespace ControleFinanceiro.Application.Cartoes.Commands.UpdateCartao;

public class UpdateCartaoCommandValidator : AbstractValidator<UpdateCartaoCommand>
{
    public UpdateCartaoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.DiaVencimento)
            .InclusiveBetween(1, 31).WithMessage("Dia de vencimento deve estar entre 1 e 31.")
            .When(x => x.DiaVencimento.HasValue);
    }
}
