using ControleFinanceiro.Application.Consultoria.Commands.SaveConsultoriaConfig;
using ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaConfig;
using ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaLogo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

/// <summary>Marca/identidade da consultoria do assessor (logo, nome, cor, WhatsApp, rodapé do relatório).</summary>
public record ConsultoriaConfigRequest(
    string NomeConsultoria,
    string? LogoBase64,
    string? CorMarca,
    string? WhatsApp,
    string? MensagemRodape);

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ConsultoriaController(IMediator mediator) : ControllerBase
{
    /// <summary>Configuração da consultoria do assessor logado.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetConsultoriaConfigQuery(), cancellationToken));

    /// <summary>Salva (upsert) a configuração da consultoria.</summary>
    [HttpPut]
    public async Task<IActionResult> Salvar([FromBody] ConsultoriaConfigRequest req, CancellationToken cancellationToken)
    {
        await mediator.Send(new SaveConsultoriaConfigCommand(
            req.NomeConsultoria, req.LogoBase64, req.CorMarca, req.WhatsApp, req.MensagemRodape), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Público: serve a logo da consultoria como imagem, para uso em e-mails
    /// (clientes de e-mail bloqueiam base64 embutido, mas carregam URL pública).
    /// </summary>
    [HttpGet("{assessorId:guid}/logo")]
    [AllowAnonymous]
    public async Task<IActionResult> Logo(Guid assessorId, CancellationToken cancellationToken)
    {
        var logo = await mediator.Send(new GetConsultoriaLogoQuery(assessorId), cancellationToken);
        if (logo is null) return NotFound();
        Response.Headers.CacheControl = "public, max-age=3600";
        return File(logo.Bytes, logo.ContentType);
    }
}
