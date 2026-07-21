using ControleFinanceiro.Application.Patrimonio.Commands.Estruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

public record EstruturaRequest(
    string Nome,
    TipoEstrutura Tipo,
    string? Jurisdicao,
    DateTime? ConstituidaEm,
    string? Observacoes);

public record ParticipacaoRequest(
    Guid? EstruturaPaiId,
    Guid EstruturaFilhaId,
    decimal PercentualParticipacao,
    TipoRelacaoEstrutura TipoRelacao);

/// <summary>Estruturas patrimoniais/sucessórias do cliente (trust, holding, offshore) + grafo de participações.</summary>
[ApiController]
[Authorize]
[Route("api/estruturas")]
public class EstruturasController(IMediator mediator) : ControllerBase
{
    /// <summary>Grafo completo: estruturas com valores derivados + participações.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await mediator.Send(new GetEstruturasQuery(), ct));

    /// <summary>Detalhe de uma estrutura: ativos/investimentos ligados + estruturas detidas.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetalhe(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetEstruturaDetalheQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EstruturaRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveEstruturaCommand(null, req.Nome, req.Tipo,
            req.Jurisdicao, req.ConstituidaEm, req.Observacoes), ct);
        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EstruturaRequest req, CancellationToken ct)
    {
        await mediator.Send(new SaveEstruturaCommand(id, req.Nome, req.Tipo,
            req.Jurisdicao, req.ConstituidaEm, req.Observacoes), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteEstruturaCommand(id), ct);
        return NoContent();
    }

    // ── Participações (arestas do grafo) ─────────────────────────────────

    [HttpPost("participacoes")]
    public async Task<IActionResult> SaveParticipacao([FromBody] ParticipacaoRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveParticipacaoCommand(req.EstruturaPaiId,
            req.EstruturaFilhaId, req.PercentualParticipacao, req.TipoRelacao), ct);
        return Ok(new { id });
    }

    [HttpDelete("participacoes/{id:guid}")]
    public async Task<IActionResult> DeleteParticipacao(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteParticipacaoCommand(id), ct);
        return NoContent();
    }
}
