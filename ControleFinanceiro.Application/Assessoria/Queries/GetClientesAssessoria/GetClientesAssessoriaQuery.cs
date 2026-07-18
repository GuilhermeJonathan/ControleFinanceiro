using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetClientesAssessoria;

public record ClienteAssessoriaDto(
    Guid VinculoId,
    Guid ClienteId,
    string? NomeCliente,
    string CodigoConvite,
    bool Aceito,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AceitoEm,
    string? AvatarUrl,
    string? Email,
    string? EmailConvidado,
    DateTime? ExpiraEm,
    bool Expirado);

public record GetClientesAssessoriaQuery : IRequest<IEnumerable<ClienteAssessoriaDto>>;

public class GetClientesAssessoriaQueryHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser,
    IUserNameLookup userLookup)
    : IRequestHandler<GetClientesAssessoriaQuery, IEnumerable<ClienteAssessoriaDto>>
{
    public async Task<IEnumerable<ClienteAssessoriaDto>> Handle(
        GetClientesAssessoriaQuery request, CancellationToken cancellationToken)
    {
        var vinculos = (await repository.GetByAssessorAsync(currentUser.RealUserId, cancellationToken))
            .Where(v => v.RevogadoEm == null)
            .ToList();

        var resultado = new List<ClienteAssessoriaDto>();
        foreach (var v in vinculos)
        {
            string? avatar = null;
            string? email = v.EmailConvidado; // pendente: e-mail do convite
            if (v.AceitoEm != null)
            {
                var contato = await userLookup.GetContatoAsync(v.ClienteId, cancellationToken);
                avatar = contato?.AvatarUrl;
                email = contato?.Email ?? v.EmailConvidado;
            }
            resultado.Add(new ClienteAssessoriaDto(
                v.Id, v.ClienteId, v.NomeCliente, v.CodigoConvite,
                v.AceitoEm != null, v.Ativo, v.CriadoEm, v.AceitoEm, avatar,
                email, v.EmailConvidado, v.ExpiraEm, v.Expirado));
        }
        return resultado;
    }
}
