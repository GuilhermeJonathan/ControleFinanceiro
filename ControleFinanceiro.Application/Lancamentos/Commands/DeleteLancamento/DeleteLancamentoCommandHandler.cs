using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteLancamento;

public class DeleteLancamentoCommandHandler(ILancamentoRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLancamentoCommand>
{
    public async Task Handle(DeleteLancamentoCommand request, CancellationToken cancellationToken)
    {
        var lancamento = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lançamento {request.Id} não encontrado.");

        repository.Delete(lancamento);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
