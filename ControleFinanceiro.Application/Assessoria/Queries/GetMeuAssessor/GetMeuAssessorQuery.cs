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
    IConsultoriaConfigRepository consultoriaRepository,
    IUserNameLookup userLookup,
    ICurrentUser currentUser)
    : IRequestHandler<GetMeuAssessorQuery, MeuAssessorDto>
{
    public async Task<MeuAssessorDto> Handle(GetMeuAssessorQuery request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByClienteAsync(currentUser.RealUserId, cancellationToken);
        if (vinculo is null)
            return new MeuAssessorDto(false, null, null, null, null);

        // Preferimos os dados da consultoria configurada; caímos no vínculo/telefone do perfil.
        var config = await consultoriaRepository.GetByUsuarioAsync(vinculo.AssessorId, cancellationToken);
        var nome = !string.IsNullOrWhiteSpace(config?.NomeConsultoria) ? config!.NomeConsultoria : vinculo.NomeAssessor;
        var whatsApp = !string.IsNullOrWhiteSpace(config?.WhatsApp)
            ? config!.WhatsApp
            : (await userLookup.GetContatoAsync(vinculo.AssessorId, cancellationToken))?.Cellphone;

        return new MeuAssessorDto(true, vinculo.Id, nome, vinculo.AceitoEm, whatsApp);
    }
}
