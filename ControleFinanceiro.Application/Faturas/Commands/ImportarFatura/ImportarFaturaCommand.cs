using MediatR;

namespace ControleFinanceiro.Application.Faturas.Commands.ImportarFatura;

public record ImportarFaturaItemDto(
    string Descricao,
    DateTime Data,
    decimal Valor,
    int Mes,
    int Ano,
    Guid CartaoId,
    string CategoriaNome,   // nome vindo do Excel; handler resolve/cria
    int? ParcelaAtual,
    int? TotalParcelas
);

public record ImportarFaturaCommand(List<ImportarFaturaItemDto> Items) : IRequest<int>;
