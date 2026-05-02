using FluentValidation;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteGrupoParcelas;

public class DeleteGrupoParcelasCommandValidator : AbstractValidator<DeleteGrupoParcelasCommand>
{
    public DeleteGrupoParcelasCommandValidator()
    {
        RuleFor(x => x.GrupoParcelas)
            .NotEmpty().WithMessage("GrupoParcelas é obrigatório.");
    }
}
