using FluentValidation;

namespace ControleFinanceiro.Application.Faturas.Commands.ImportarFatura;

public class ImportarFaturaCommandValidator : AbstractValidator<ImportarFaturaCommand>
{
    public ImportarFaturaCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("A lista de itens não pode estar vazia.");

        RuleForEach(x => x.Items).SetValidator(new ImportarFaturaItemDtoValidator());
    }
}

public class ImportarFaturaItemDtoValidator : AbstractValidator<ImportarFaturaItemDto>
{
    public ImportarFaturaItemDtoValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição do item é obrigatória.")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Valor)
            .NotEqual(0).WithMessage("Valor do item não pode ser zero.");

        RuleFor(x => x.Mes)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12.");

        RuleFor(x => x.Ano)
            .GreaterThan(2000).WithMessage("Ano inválido.");

        RuleFor(x => x.CartaoId)
            .NotEmpty().WithMessage("CartaoId é obrigatório.");

        RuleFor(x => x.CategoriaNome)
            .NotEmpty().WithMessage("Nome da categoria é obrigatório.")
            .MaximumLength(100).WithMessage("Nome da categoria deve ter no máximo 100 caracteres.");
    }
}
