using FluentValidation;

namespace ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamento;

public class UpdateLancamentoCommandValidator : AbstractValidator<UpdateLancamentoCommand>
{
    public UpdateLancamentoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Valor)
            .NotEqual(0).WithMessage("Valor não pode ser zero.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de lançamento inválido.");

        RuleFor(x => x.Situacao)
            .IsInEnum().WithMessage("Situação inválida.");
    }
}
