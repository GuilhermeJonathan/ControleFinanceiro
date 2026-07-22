using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.UpdateInvestimento;

public record UpdateInvestimentoCommand(
    Guid Id,
    string Nome,
    TipoInvestimento Tipo,
    MoedaPatrimonio Moeda,
    string? Corretora,
    string? Ticker,
    decimal ValorAplicado,
    decimal ValorAtual,
    decimal? RentabilidadeAnualPct,
    decimal? Quantidade = null,
    Guid? EstruturaId = null,
    Guid? ContaId = null,
    string? Subclasse = null) : IRequest;

public class UpdateInvestimentoCommandHandler(
    IInvestimentoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateInvestimentoCommand>
{
    public async Task Handle(UpdateInvestimentoCommand request, CancellationToken cancellationToken)
    {
        var inv = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Investimento {request.Id} nao encontrado.");

        if (inv.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao investimento.");

        inv.Atualizar(request.Nome, request.Tipo, request.Moeda, request.Corretora, request.Ticker,
            request.ValorAplicado, request.ValorAtual, request.RentabilidadeAnualPct, request.Quantidade,
            request.EstruturaId, request.ContaId, request.Subclasse);

        repository.Update(inv);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
