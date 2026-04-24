using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.CreateParcela;

public record CreateParcelaCommand(
    Guid CartaoCreditoId,
    string Descricao,
    decimal ValorParcela,
    int ParcelaAtual,
    int TotalParcelas,
    DateTime DataInicio
) : IRequest<Guid>;
