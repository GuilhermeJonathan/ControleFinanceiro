using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetConvitesHistorico;

public record ConviteHistoricoDto(
    Guid VinculoId,
    string CodigoConvite,
    string Status,           // Pendente | Aceito | Revogado
    string? NomeCliente,
    DateTime CriadoEm,
    DateTime? AceitoEm,
    DateTime? RevogadoEm);

/// <summary>Histórico completo de convites do assessor, incluindo aceitos e revogados.</summary>
public record GetConvitesHistoricoQuery : IRequest<IEnumerable<ConviteHistoricoDto>>;

public class GetConvitesHistoricoQueryHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetConvitesHistoricoQuery, IEnumerable<ConviteHistoricoDto>>
{
    public async Task<IEnumerable<ConviteHistoricoDto>> Handle(
        GetConvitesHistoricoQuery request, CancellationToken cancellationToken)
    {
        var vinculos = await repository.GetByAssessorAsync(currentUser.RealUserId, cancellationToken);

        return vinculos.Select(v => new ConviteHistoricoDto(
            v.Id,
            v.CodigoConvite,
            v.RevogadoEm != null ? "Revogado" : v.AceitoEm != null ? "Aceito" : "Pendente",
            v.NomeCliente,
            v.CriadoEm,
            v.AceitoEm,
            v.RevogadoEm));
    }
}
