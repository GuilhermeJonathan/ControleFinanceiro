using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Simulacoes.Commands.UpdateSimulacao;

public record UpdateSimulacaoCommand(
    Guid Id,
    string Nome,
    bool Favorita,
    int IdadeAtual,
    int IdadeAlvo,
    decimal PatrimonioInicial,
    bool ModoAutomatico,
    decimal AporteMensal,
    decimal TaxaRetornoRealAnualPct,
    decimal RetiradaMensal,
    IReadOnlyList<CenarioInput> Cenarios) : IRequest;

public class UpdateSimulacaoCommandHandler(
    ISimulacaoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSimulacaoCommand>
{
    public async Task Handle(UpdateSimulacaoCommand request, CancellationToken cancellationToken)
    {
        var simulacao = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Simulação {request.Id} não encontrada.");

        if (simulacao.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado à simulação.");

        simulacao.Atualizar(
            request.Nome, request.Favorita, request.IdadeAtual, request.IdadeAlvo,
            request.PatrimonioInicial, request.ModoAutomatico, request.AporteMensal,
            request.TaxaRetornoRealAnualPct, request.RetiradaMensal,
            request.Cenarios?.Select(c => c.ToEntity()));

        repository.Update(simulacao);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
