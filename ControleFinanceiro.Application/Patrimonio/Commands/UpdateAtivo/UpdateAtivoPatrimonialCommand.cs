using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.UpdateAtivo;

public record UpdateAtivoPatrimonialCommand(
    Guid Id,
    string Nome,
    TipoAtivo Tipo,
    MoedaPatrimonio Moeda,
    decimal ValorAtual,
    decimal? ValorizacaoAnualPct) : IRequest;

public class UpdateAtivoPatrimonialCommandHandler(
    IAtivoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAtivoPatrimonialCommand>
{
    public async Task Handle(UpdateAtivoPatrimonialCommand request, CancellationToken cancellationToken)
    {
        var ativo = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Ativo {request.Id} não encontrado.");

        if (ativo.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao ativo.");

        ativo.Atualizar(request.Nome, request.Tipo, request.Moeda, request.ValorAtual, request.ValorizacaoAnualPct);
        repository.Update(ativo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
