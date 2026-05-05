using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.ImportarExtrato;

public record ImportarExtratoItem(
    string   Descricao,
    decimal  Valor,
    DateTime Data,
    int      Mes,
    int      Ano,
    string?  CategoriaNome,
    Guid?    ContaBancariaId
);

public record ImportarExtratoCommand(
    List<ImportarExtratoItem> Items
) : IRequest<int>;
