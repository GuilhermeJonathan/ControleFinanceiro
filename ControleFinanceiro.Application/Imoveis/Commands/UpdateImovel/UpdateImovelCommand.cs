using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Commands.UpdateImovel;

public record UpdateImovelCommand(
    Guid Id,
    string Descricao,
    decimal Valor,
    List<string> Pros,
    List<string> Contras,
    int Nota,
    DateTime DataVisita,
    string? NomeCorretor,
    string? TelefoneCorretor,
    string? Imobiliaria,
    string? Tipo) : IRequest;

public class UpdateImovelCommandHandler(
    IImovelRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<UpdateImovelCommand>
{
    public async Task Handle(UpdateImovelCommand r, CancellationToken ct)
    {
        var imovel = await repo.GetByIdAsync(r.Id, currentUser.UserId, currentUser.PodeVerImoveis, ct)
            ?? throw new KeyNotFoundException("Imóvel não encontrado.");

        imovel.Update(r.Descricao, r.Valor, r.Pros, r.Contras,
            r.Nota, r.DataVisita, r.NomeCorretor,
            r.TelefoneCorretor, r.Imobiliaria, r.Tipo);

        repo.Update(imovel);
        await uow.SaveChangesAsync(ct);
    }
}
