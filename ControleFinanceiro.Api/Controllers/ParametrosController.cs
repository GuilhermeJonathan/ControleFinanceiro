using ControleFinanceiro.Application.Parametros.Commands;
using ControleFinanceiro.Application.Parametros.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

/// <summary>
/// Parâmetros configuráveis pelo Assessor: TipoAtivo, TipoInvestimento e Moeda.
/// GET é público (para popular selects dos clientes); POST/PUT/DELETE exige Assessor.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParametrosController(IMediator mediator) : ControllerBase
{
    // ── Tipos de Ativo ────────────────────────────────────────────────────

    [HttpGet("tipos-ativo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTiposAtivo(CancellationToken ct) =>
        Ok(await mediator.Send(new GetTiposAtivoQuery(), ct));

    [HttpPost("tipos-ativo")]
    [Authorize]
    public async Task<IActionResult> SaveTipoAtivo([FromBody] SaveParamRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveTipoAtivoCommand(req.Id, req.Nome, req.Icone, req.Ordem, req.Ativo), ct);
        return Ok(new { id });
    }

    [HttpDelete("tipos-ativo/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteTipoAtivo(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTipoAtivoCommand(id), ct);
        return NoContent();
    }

    // ── Tipos de Investimento ─────────────────────────────────────────────

    [HttpGet("tipos-investimento")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTiposInvestimento(CancellationToken ct) =>
        Ok(await mediator.Send(new GetTiposInvestimentoQuery(), ct));

    [HttpPost("tipos-investimento")]
    [Authorize]
    public async Task<IActionResult> SaveTipoInvestimento([FromBody] SaveParamRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveTipoInvestimentoCommand(req.Id, req.Nome, req.Icone, req.Ordem, req.Ativo), ct);
        return Ok(new { id });
    }

    [HttpDelete("tipos-investimento/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteTipoInvestimento(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTipoInvestimentoCommand(id), ct);
        return NoContent();
    }

    // ── Moedas ────────────────────────────────────────────────────────────

    [HttpGet("moedas")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMoedas(CancellationToken ct) =>
        Ok(await mediator.Send(new GetMoedasQuery(), ct));

    [HttpPost("moedas")]
    [Authorize]
    public async Task<IActionResult> SaveMoeda([FromBody] SaveMoedaRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveMoedaCommand(req.Id, req.Codigo, req.Nome, req.Ordem, req.Ativo), ct);
        return Ok(new { id });
    }

    [HttpDelete("moedas/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMoeda(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMoedaCommand(id), ct);
        return NoContent();
    }
}

public record SaveParamRequest(int? Id, string Nome, string? Icone, int Ordem, bool Ativo);
public record SaveMoedaRequest(int? Id, string Codigo, string Nome, int Ordem, bool Ativo);
