using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.CreateParcela;

public class CreateParcelaCommandHandler(IParcelaCartaoRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateParcelaCommand, Guid>
{
    public async Task<Guid> Handle(CreateParcelaCommand request, CancellationToken cancellationToken)
    {
        var parcela = new ParcelaCartao(
            request.CartaoCreditoId, request.Descricao, request.ValorParcela,
            request.ParcelaAtual, request.TotalParcelas, request.DataInicio);

        await repository.AddAsync(parcela, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return parcela.Id;
    }
}
