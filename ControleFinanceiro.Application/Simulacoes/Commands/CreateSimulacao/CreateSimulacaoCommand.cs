using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Simulacoes.Commands.CreateSimulacao;

public record CreateSimulacaoCommand(
    string Nome,
    bool Favorita,
    int IdadeAtual,
    int IdadeAlvo,
    decimal PatrimonioInicial,
    bool ModoAutomatico,
    decimal AporteMensal,
    decimal TaxaRetornoRealAnualPct,
    decimal RetiradaMensal,
    IReadOnlyList<CenarioInput> Cenarios) : IRequest<Guid>;

public class CreateSimulacaoCommandHandler(
    ISimulacaoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSimulacaoCommand, Guid>
{
    public async Task<Guid> Handle(CreateSimulacaoCommand request, CancellationToken cancellationToken)
    {
        var simulacao = new SimulacaoPatrimonial(
            currentUser.UserId,
            request.Nome,
            request.Favorita,
            request.IdadeAtual,
            request.IdadeAlvo,
            request.PatrimonioInicial,
            request.ModoAutomatico,
            request.AporteMensal,
            request.TaxaRetornoRealAnualPct,
            request.RetiradaMensal,
            request.Cenarios?.Select(c => c.ToEntity()));

        await repository.AddAsync(simulacao, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return simulacao.Id;
    }
}
