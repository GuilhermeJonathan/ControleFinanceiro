using FluentValidation;

namespace ControleFinanceiro.Application.Metas.Commands.CreateMeta;

public class CreateMetaCommandValidator : AbstractValidator<CreateMetaCommand>
{
    public CreateMetaCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ValorMeta)
            .GreaterThan(0).WithMessage("Valor da meta deve ser maior que zero.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres.")
            .When(x => x.Descricao != null);
    }
}
