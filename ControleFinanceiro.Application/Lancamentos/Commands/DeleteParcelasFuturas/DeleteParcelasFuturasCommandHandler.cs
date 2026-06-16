using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteParcelasFuturas;

public class DeleteParcelasFuturasCommandHandler(
    ILancamentoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteParcelasFuturasCommand>
{
    public async Task Handle(DeleteParcelasFuturasCommand request, CancellationToken cancellationToken)
    {
        var parcelas = await repository.GetByGrupoParcelasFromAsync(
            request.GrupoParcelas, request.ParcelaAtualFrom, currentUser.UserId, cancellationToken);

        var lista = parcelas.ToList();

        // Recorrentes: soft-cancel para o DailyJobService não regenerar os meses excluídos
        foreach (var p in lista.Where(p => p.IsRecorrente))
            p.AtualizarSituacao(SituacaoLancamento.Cancelado);

        // Não-recorrentes: hard delete normal
        repository.DeleteRange(lista.Where(p => !p.IsRecorrente));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
