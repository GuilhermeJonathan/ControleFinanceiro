using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.CreatePassivo;

public record CreatePassivoPatrimonialCommand(
    string Nome,
    MoedaPatrimonio Moeda,
    decimal Valor,
    PrazoDivida Prazo,
    decimal? TaxaJurosAnualPct = null,
    int? PrazoMeses = null) : IRequest<Guid>;

public class CreatePassivoPatrimonialCommandHandler(
    IPassivoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePassivoPatrimonialCommand, Guid>
{
    public async Task<Guid> Handle(CreatePassivoPatrimonialCommand request, CancellationToken cancellationToken)
    {
        var passivo = new PassivoPatrimonial(
            currentUser.UserId,
            request.Nome,
            request.Moeda,
            request.Valor,
            request.Prazo,
            request.TaxaJurosAnualPct,
            request.PrazoMeses);

        await repository.AddAsync(passivo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return passivo.Id;
    }
}
