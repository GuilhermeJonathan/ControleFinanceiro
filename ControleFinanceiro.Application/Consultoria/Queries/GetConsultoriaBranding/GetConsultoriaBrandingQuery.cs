using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaBranding;

/// <summary>Marca pública de uma consultoria (login whitelabel via ?a={assessorId|slug}).</summary>
public record ConsultoriaBrandingDto(Guid AssessorId, string? NomeConsultoria, string? CorMarca, bool TemLogo, string? Slug);

public record GetConsultoriaBrandingQuery(Guid AssessorId) : IRequest<ConsultoriaBrandingDto?>;

/// <summary>Resolve a marca pela rota/slug definida no admin.</summary>
public record GetConsultoriaBrandingBySlugQuery(string Slug) : IRequest<ConsultoriaBrandingDto?>;

public class GetConsultoriaBrandingQueryHandler(IConsultoriaConfigRepository repo)
    : IRequestHandler<GetConsultoriaBrandingQuery, ConsultoriaBrandingDto?>,
      IRequestHandler<GetConsultoriaBrandingBySlugQuery, ConsultoriaBrandingDto?>
{
    public async Task<ConsultoriaBrandingDto?> Handle(GetConsultoriaBrandingQuery request, CancellationToken ct) =>
        Map(await repo.GetByUsuarioAsync(request.AssessorId, ct));

    public async Task<ConsultoriaBrandingDto?> Handle(GetConsultoriaBrandingBySlugQuery request, CancellationToken ct)
    {
        var slug = ConsultoriaConfig.NormalizarSlug(request.Slug);
        return slug is null ? null : Map(await repo.GetBySlugAsync(slug, ct));
    }

    private static ConsultoriaBrandingDto? Map(ConsultoriaConfig? config) =>
        config is null ? null : new ConsultoriaBrandingDto(
            config.UsuarioId, config.NomeConsultoria, config.CorMarca,
            !string.IsNullOrWhiteSpace(config.LogoBase64), config.Slug);
}
