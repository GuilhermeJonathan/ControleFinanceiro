using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaConfig;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Admin.Queries.GetAssessoriaConsultoria;

/// <summary>Marca (consultoria) de uma assessoria específica — para o admin editar (prefill). Admin-only.</summary>
public record GetAssessoriaConsultoriaQuery(Guid AssessorId) : IRequest<ConsultoriaConfigDto>;

public class GetAssessoriaConsultoriaQueryHandler(
    IConsultoriaConfigRepository repo,
    ICurrentUser currentUser)
    : IRequestHandler<GetAssessoriaConsultoriaQuery, ConsultoriaConfigDto>
{
    public async Task<ConsultoriaConfigDto> Handle(GetAssessoriaConsultoriaQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas o admin da plataforma pode consultar a marca de outras assessorias.");

        var c = await repo.GetByUsuarioAsync(request.AssessorId, ct);
        return c is null
            ? new ConsultoriaConfigDto("", null, null, null, null)
            : new ConsultoriaConfigDto(c.NomeConsultoria, c.LogoBase64, c.CorMarca, c.WhatsApp, c.MensagemRodape, c.Slug);
    }
}
