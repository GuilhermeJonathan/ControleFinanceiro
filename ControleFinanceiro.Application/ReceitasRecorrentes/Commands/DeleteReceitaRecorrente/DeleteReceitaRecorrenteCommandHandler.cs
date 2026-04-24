using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.DeleteReceitaRecorrente;

public class DeleteReceitaRecorrenteCommandHandler(
    IReceitaRecorrenteRepository receitaRepository,
    ILancamentoRepository lancamentoRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteReceitaRecorrenteCommand>
{
    public async Task Handle(DeleteReceitaRecorrenteCommand request, CancellationToken cancellationToken)
    {
        var receita = await receitaRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"ReceitaRecorrente {request.Id} não encontrada.");

        var hoje = DateTime.Today;

        // Remove todos os lançamentos futuros (mês atual em diante)
        var futuros = await lancamentoRepository.GetFutureByReceitaRecorrenteIdAsync(
            request.Id, hoje.Month, hoje.Year, cancellationToken);

        lancamentoRepository.DeleteRange(futuros);
        receitaRepository.Delete(receita);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
