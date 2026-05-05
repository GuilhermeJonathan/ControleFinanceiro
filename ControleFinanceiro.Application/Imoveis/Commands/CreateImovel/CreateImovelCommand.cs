using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Commands.CreateImovel;

public record CreateImovelCommand(
    string Descricao,
    decimal Valor,
    List<string> Pros,
    List<string> Contras,
    int Nota,
    DateTime DataVisita,
    string? NomeCorretor,
    string? TelefoneCorretor,
    string? Imobiliaria,
    string? Tipo) : IRequest<Guid>;

public class CreateImovelCommandHandler(
    IImovelRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<CreateImovelCommand, Guid>
{
    public async Task<Guid> Handle(CreateImovelCommand r, CancellationToken ct)
    {
        var imovel = new Imovel(
            r.Descricao, r.Valor, r.Pros, r.Contras,
            r.Nota, r.DataVisita, r.NomeCorretor,
            r.TelefoneCorretor, r.Imobiliaria, r.Tipo, currentUser.UserId);

        await repo.AddAsync(imovel, ct);
        await uow.SaveChangesAsync(ct);
        return imovel.Id;
    }
}
