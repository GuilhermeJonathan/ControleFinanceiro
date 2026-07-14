using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaConfig;

public record ConsultoriaConfigDto(
    string NomeConsultoria,
    string? LogoBase64,
    string? CorMarca,
    string? WhatsApp,
    string? MensagemRodape);

public record GetConsultoriaConfigQuery : IRequest<ConsultoriaConfigDto>;

public class GetConsultoriaConfigQueryHandler(
    IConsultoriaConfigRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetConsultoriaConfigQuery, ConsultoriaConfigDto>
{
    public async Task<ConsultoriaConfigDto> Handle(GetConsultoriaConfigQuery request, CancellationToken cancellationToken)
    {
        var config = await repository.GetByUsuarioAsync(currentUser.RealUserId, cancellationToken);

        return config is null
            ? new ConsultoriaConfigDto(currentUser.RealUserName ?? "", null, null, null, null)
            : new ConsultoriaConfigDto(config.NomeConsultoria, config.LogoBase64, config.CorMarca, config.WhatsApp, config.MensagemRodape);
    }
}
