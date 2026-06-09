using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteLancamento;

public class DeleteLancamentoCommandHandler(
    ILancamentoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteLancamentoCommand>
{
    public async Task Handle(DeleteLancamentoCommand request, CancellationToken cancellationToken)
    {
        var lancamento = await repository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lançamento {request.Id} não encontrado.");

        if (lancamento.IsRecorrente)
        {
            lancamento.AtualizarSituacao(SituacaoLancamento.Cancelado);
        }
        else
        {
            repository.Delete(lancamento);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
