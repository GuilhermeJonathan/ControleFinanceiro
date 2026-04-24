using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamento;

public class UpdateLancamentoCommandHandler(ILancamentoRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLancamentoCommand>
{
    public async Task Handle(UpdateLancamentoCommand request, CancellationToken cancellationToken)
    {
        var lancamento = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lançamento {request.Id} não encontrado.");

        lancamento.Update(request.Descricao, request.Data, request.Valor,
            request.Tipo, request.Situacao, request.CategoriaId, request.CartaoId);

        repository.Update(lancamento);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
