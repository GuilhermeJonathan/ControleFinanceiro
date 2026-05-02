using FluentValidation;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.DeleteReceitaRecorrente;

public class DeleteReceitaRecorrenteCommandValidator : AbstractValidator<DeleteReceitaRecorrenteCommand>
{
    public DeleteReceitaRecorrenteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");
    }
}
