using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.CreateInvestimento;

public record CreateInvestimentoCommand(
    string Nome,
    TipoInvestimento Tipo,
    MoedaPatrimonio Moeda,
    string? Corretora,
    string? Ticker,
    decimal ValorAplicado,
    decimal ValorAtual,
    decimal? RentabilidadeAnualPct,
    decimal? Quantidade = null) : IRequest<Guid>;

public class CreateInvestimentoCommandHandler(
    IInvestimentoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateInvestimentoCommand, Guid>
{
    public async Task<Guid> Handle(CreateInvestimentoCommand request, CancellationToken cancellationToken)
    {
        var inv = new Investimento(
            currentUser.UserId,
            request.Nome,
            request.Tipo,
            request.Moeda,
            request.Corretora,
            request.Ticker,
            request.ValorAplicado,
            request.ValorAtual,
            request.RentabilidadeAnualPct,
            request.Quantidade);

        await repository.AddAsync(inv, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return inv.Id;
    }
}
