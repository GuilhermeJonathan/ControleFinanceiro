using FluentValidation;

namespace ControleFinanceiro.Application.Metas.Commands.DeleteMeta;

public class DeleteMetaCommandValidator : AbstractValidator<DeleteMetaCommand>
{
    public DeleteMetaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
