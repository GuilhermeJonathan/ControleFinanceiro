using System.Text.RegularExpressions;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaLogo;

public record ConsultoriaLogoDto(byte[] Bytes, string ContentType);

/// <summary>
/// Retorna a logo da consultoria de um assessor como bytes + content-type, para ser
/// servida por uma URL pública (e-mails/relatórios). Retorna null se não houver logo.
/// </summary>
public record GetConsultoriaLogoQuery(Guid AssessorId) : IRequest<ConsultoriaLogoDto?>;

public partial class GetConsultoriaLogoQueryHandler(IConsultoriaConfigRepository repo)
    : IRequestHandler<GetConsultoriaLogoQuery, ConsultoriaLogoDto?>
{
    public async Task<ConsultoriaLogoDto?> Handle(GetConsultoriaLogoQuery request, CancellationToken cancellationToken)
    {
        var config = await repo.GetByUsuarioAsync(request.AssessorId, cancellationToken);
        return Parse(config?.LogoBase64);
    }

    /// <summary>Aceita data URL ("data:image/png;base64,....") ou base64 puro (assume PNG).</summary>
    public static ConsultoriaLogoDto? Parse(string? logo)
    {
        if (string.IsNullOrWhiteSpace(logo)) return null;

        var contentType = "image/png";
        var base64 = logo.Trim();

        var m = DataUrlRegex().Match(base64);
        if (m.Success)
        {
            contentType = m.Groups["ct"].Value;
            base64 = m.Groups["data"].Value;
        }

        try
        {
            var bytes = Convert.FromBase64String(base64);
            return bytes.Length == 0 ? null : new ConsultoriaLogoDto(bytes, contentType);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    [GeneratedRegex(@"^data:(?<ct>[\w/+.-]+);base64,(?<data>.+)$", RegexOptions.Singleline)]
    private static partial Regex DataUrlRegex();
}
