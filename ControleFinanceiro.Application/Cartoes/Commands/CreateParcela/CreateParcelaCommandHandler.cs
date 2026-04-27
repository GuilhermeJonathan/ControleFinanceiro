using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.CreateParcela;

public class CreateParcelaCommandHandler(
    IParcelaCartaoRepository repository,
    ICartaoCreditoRepository cartaoRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateParcelaCommand, Guid>
{
    public async Task<Guid> Handle(CreateParcelaCommand request, CancellationToken cancellationToken)
    {
        _ = await cartaoRepository.GetByIdAsync(request.CartaoCreditoId, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cartão {request.CartaoCreditoId} não encontrado.");

        var parcela = new ParcelaCartao(
            request.CartaoCreditoId, request.Descricao, request.ValorParcela,
            request.ParcelaAtual, request.TotalParcelas, request.DataInicio);

        await repository.AddAsync(parcela, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return parcela.Id;
    }
}
