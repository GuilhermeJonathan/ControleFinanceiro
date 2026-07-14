using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetMeuAssessor;

public record MeuAssessorDto(
    bool TemAssessor,
    Guid? VinculoId,
    string? NomeAssessor,
    DateTime? AceitoEm,
    string? WhatsApp);

public record GetMeuAssessorQuery : IRequest<MeuAssessorDto>;

public class GetMeuAssessorQueryHandler(
    IVinculoAssessoriaRepository repository,
    IUserNameLookup userLookup,
    ICurrentUser currentUser)
    : IRequestHandler<GetMeuAssessorQuery, MeuAssessorDto>
{
    public async Task<MeuAssessorDto> Handle(GetMeuAssessorQuery request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByClienteAsync(currentUser.RealUserId, cancellationToken);
        if (vinculo is null)
            return new MeuAssessorDto(false, null, null, null, null);

        // WhatsApp do assessor (telefone cadastrado no perfil dele).
        var contato = await userLookup.GetContatoAsync(vinculo.AssessorId, cancellationToken);

        return new MeuAssessorDto(true, vinculo.Id, vinculo.NomeAssessor, vinculo.AceitoEm, contato?.Cellphone);
    }
}
