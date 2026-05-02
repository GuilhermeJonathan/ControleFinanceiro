using FluentValidation;

namespace ControleFinanceiro.Application.Horas.Commands.DeleteHoras;

public class DeleteHorasCommandValidator : AbstractValidator<DeleteHorasCommand>
{
    public DeleteHorasCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
