using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamentoRecorrenteFuturas;

public class UpdateLancamentoRecorrenteFuturasCommandHandler(
    ILancamentoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateLancamentoRecorrenteFuturasCommand>
{
    public async Task Handle(UpdateLancamentoRecorrenteFuturasCommand request, CancellationToken cancellationToken)
    {
        var lancamento = await repository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lançamento {request.Id} não encontrado.");

        // Atualiza o lançamento atual completo (incluindo situação)
        lancamento.Update(request.Descricao, request.Data, request.Valor,
            request.Tipo, request.Situacao, request.CategoriaId, request.CartaoId);

        // Atualiza todos os futuros do mesmo grupo (só campos template — preserva mês/ano/situação)
        if (lancamento.GrupoParcelas.HasValue && lancamento.ParcelaAtual.HasValue)
        {
            var futuras = await repository.GetByGrupoParcelasFromAsync(
                lancamento.GrupoParcelas.Value,
                lancamento.ParcelaAtual.Value + 1,   // exclui o atual, já atualizado acima
                currentUser.UserId,
                cancellationToken);

            foreach (var f in futuras)
                f.UpdateRecorrente(request.Descricao, request.Valor,
                    request.Tipo, request.CategoriaId, request.CartaoId, request.Data.Day);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
