using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetMeuAssessor;

public record MeuAssessorDto(
    bool TemAssessor,
    Guid? VinculoId,
    string? NomeAssessor,
    DateTime? AceitoEm);

public record GetMeuAssessorQuery : IRequest<MeuAssessorDto>;

public class GetMeuAssessorQueryHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetMeuAssessorQuery, MeuAssessorDto>
{
    public async Task<MeuAssessorDto> Handle(GetMeuAssessorQuery request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByClienteAsync(currentUser.RealUserId, cancellationToken);

        return vinculo is null
            ? new MeuAssessorDto(false, null, null, null)
            : new MeuAssessorDto(true, vinculo.Id, vinculo.NomeAssessor, vinculo.AceitoEm);
    }
}
