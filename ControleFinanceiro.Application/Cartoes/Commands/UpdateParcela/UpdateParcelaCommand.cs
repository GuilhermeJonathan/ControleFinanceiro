using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.UpdateParcela;

public record UpdateParcelaCommand(
    Guid Id,
    string Descricao,
    decimal ValorParcela,
    int ParcelaAtual,
    int TotalParcelas
) : IRequest;
