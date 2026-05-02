using FluentValidation;

namespace ControleFinanceiro.Application.Categorias.Commands.DeleteCategoria;

public class DeleteCategoriaCommandValidator : AbstractValidator<DeleteCategoriaCommand>
{
    public DeleteCategoriaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
