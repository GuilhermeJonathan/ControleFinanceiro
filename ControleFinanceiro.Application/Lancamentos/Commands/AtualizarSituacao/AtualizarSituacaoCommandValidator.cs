using FluentValidation;

namespace ControleFinanceiro.Application.Lancamentos.Commands.AtualizarSituacao;

public class AtualizarSituacaoCommandValidator : AbstractValidator<AtualizarSituacaoCommand>
{
    public AtualizarSituacaoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Situacao)
            .IsInEnum().WithMessage("Situação inválida.");
    }
}
