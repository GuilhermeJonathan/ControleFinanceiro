using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaConfig;

public record ConsultoriaConfigDto(
    string NomeConsultoria,
    string? LogoBase64,
    string? CorMarca,
    string? WhatsApp,
    string? MensagemRodape,
    string? Slug = null);

public record GetConsultoriaConfigQuery : IRequest<ConsultoriaConfigDto>;

public class GetConsultoriaConfigQueryHandler(
    IConsultoriaConfigRepository repository,
    IAssessoriaOwnerResolver ownerResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetConsultoriaConfigQuery, ConsultoriaConfigDto>
{
    public async Task<ConsultoriaConfigDto> Handle(GetConsultoriaConfigQuery request, CancellationToken cancellationToken)
    {
        var config = await repository.GetByUsuarioAsync(currentUser.RealUserId, cancellationToken);

        // Cliente/corretor sem config própria herda a marca do assessor dono (whitelabel).
        if (config is null)
        {
            var owner = await ownerResolver.ResolveOwnerAsync(cancellationToken);
            if (owner is { } assessorId && assessorId != currentUser.RealUserId)
                config = await repository.GetByUsuarioAsync(assessorId, cancellationToken);
        }

        return config is null
            ? new ConsultoriaConfigDto(currentUser.RealUserName ?? "", null, null, null, null)
            : new ConsultoriaConfigDto(config.NomeConsultoria, config.LogoBase64, config.CorMarca, config.WhatsApp, config.MensagemRodape, config.Slug);
    }
}
