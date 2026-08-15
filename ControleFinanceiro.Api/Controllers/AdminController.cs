using ControleFinanceiro.Application.Admin.Commands;
using ControleFinanceiro.Application.Admin.Queries.GetAdminOverview;
using ControleFinanceiro.Application.Admin.Queries.GetAssessoriaConsultoria;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

public record CriarAssessoriaRequest(string Nome, string Email, string Senha, string? NomeConsultoria);
public record AtualizarAssessoriaRequest(
    string NomeConsultoria, string? LogoBase64, string? CorMarca, string? WhatsApp, string? MensagemRodape, string? Slug);

/// <summary>Painel do admin da plataforma (acima dos assessores). Acesso restrito a userType=1.</summary>
[ApiController]
[Authorize]
[Route("api/admin")]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary>Visão consolidada: totais da plataforma + lista de assessorias.</summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAdminOverviewQuery(), ct));

    /// <summary>Cria uma nova assessoria (provisiona o assessor no Login + semeia a marca).</summary>
    [HttpPost("assessorias")]
    public async Task<IActionResult> CriarAssessoria([FromBody] CriarAssessoriaRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CriarAssessoriaCommand(req.Nome, req.Email, req.Senha, req.NomeConsultoria), ct);
        return Ok(new { id });
    }

    /// <summary>Marca (consultoria) atual de uma assessoria, para o admin editar (prefill).</summary>
    [HttpGet("assessorias/{id:guid}/consultoria")]
    public async Task<IActionResult> GetAssessoriaConsultoria(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetAssessoriaConsultoriaQuery(id), ct));

    /// <summary>Edita a marca completa de uma assessoria (nome, logo, cor, WhatsApp, rodapé).</summary>
    [HttpPut("assessorias/{id:guid}")]
    public async Task<IActionResult> AtualizarAssessoria(Guid id, [FromBody] AtualizarAssessoriaRequest req, CancellationToken ct)
    {
        await mediator.Send(new AtualizarAssessoriaCommand(id, req.NomeConsultoria,
            req.LogoBase64, req.CorMarca, req.WhatsApp, req.MensagemRodape, req.Slug), ct);
        return NoContent();
    }
}
