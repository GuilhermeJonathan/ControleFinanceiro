using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.UpdatePassivo;

public record UpdatePassivoPatrimonialCommand(
    Guid Id,
    string Nome,
    MoedaPatrimonio Moeda,
    decimal Valor,
    PrazoDivida Prazo,
    decimal? TaxaJurosAnualPct = null,
    int? PrazoMeses = null) : IRequest;

public class UpdatePassivoPatrimonialCommandHandler(
    IPassivoPatrimonialRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePassivoPatrimonialCommand>
{
    public async Task Handle(UpdatePassivoPatrimonialCommand request, CancellationToken cancellationToken)
    {
        var passivo = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Passivo {request.Id} não encontrado.");

        if (passivo.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao passivo.");

        passivo.Atualizar(request.Nome, request.Moeda, request.Valor, request.Prazo,
            request.TaxaJurosAnualPct, request.PrazoMeses);
        repository.Update(passivo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
