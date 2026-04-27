using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.UpdateReceitaRecorrente;

public class UpdateReceitaRecorrenteCommandHandler(
    IReceitaRecorrenteRepository receitaRepository,
    ILancamentoRepository lancamentoRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateReceitaRecorrenteCommand>
{
    public async Task Handle(UpdateReceitaRecorrenteCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;

        var receita = await receitaRepository.GetByIdAsync(request.Id, usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException($"ReceitaRecorrente {request.Id} não encontrada.");

        receita.Update(request.Nome, request.Tipo, request.Dia,
            request.Valor, request.ValorHora, request.QuantidadeHoras);
        receitaRepository.Update(receita);

        if (request.AplicarFuturos)
        {
            var hoje = DateTime.Today;
            var futuros = await lancamentoRepository.GetFutureByReceitaRecorrenteIdAsync(
                request.Id, hoje.Month, hoje.Year, usuarioId, cancellationToken);

            foreach (var l in futuros)
                l.AtualizarDeReceita(request.Nome, receita.Valor, request.Dia);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
