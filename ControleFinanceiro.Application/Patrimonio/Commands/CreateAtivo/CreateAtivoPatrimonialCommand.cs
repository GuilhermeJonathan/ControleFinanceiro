using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.CreateAtivo;

public record CreateAtivoPatrimonialCommand(
    string Nome,
    TipoAtivo Tipo,
    MoedaPatrimonio Moeda,
    decimal ValorAtual,
    decimal? ValorizacaoAnualPct) : IRequest<Guid>;

public class CreateAtivoPatrimonialCommandHandler(
    IAtivoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAtivoPatrimonialCommand, Guid>
{
    public async Task<Guid> Handle(CreateAtivoPatrimonialCommand request, CancellationToken cancellationToken)
    {
        var ativo = new AtivoPatrimonial(
            currentUser.UserId,
            request.Nome,
            request.Tipo,
            request.Moeda,
            request.ValorAtual,
            request.ValorizacaoAnualPct);

        await repository.AddAsync(ativo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ativo.Id;
    }
}
