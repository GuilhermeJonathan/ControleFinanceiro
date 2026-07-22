using ControleFinanceiro.Application.Patrimonio.Commands.Contas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

public record ContaRequest(
    string Nome,
    TipoContaFinanceira Tipo,
    string Moeda,
    decimal Saldo,
    string? Instituicao,
    string? Pais,
    string? Identificador,
    Guid? EstruturaId,
    decimal? ValorPortfolio = null,
    decimal? LombardLimite = null,
    decimal? LombardUtilizado = null,
    string? Status = null);

/// <summary>Contas financeiras do cliente (bancária, investimento/custódia, internacional).</summary>
[ApiController]
[Authorize]
[Route("api/contas")]
public class ContasController(IMediator mediator) : ControllerBase
{
    private static readonly Dictionary<string, MoedaPatrimonio> MoedaMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BRL"] = MoedaPatrimonio.BRL, ["USD"] = MoedaPatrimonio.USD, ["EUR"] = MoedaPatrimonio.EUR,
        ["CHF"] = MoedaPatrimonio.CHF, ["GBP"] = MoedaPatrimonio.GBP,
    };

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await mediator.Send(new GetContasQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContaRequest req, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(req.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {req.Moeda}.");
        var id = await mediator.Send(new SaveContaCommand(null, req.Nome, req.Tipo, moeda, req.Saldo,
            req.Instituicao, req.Pais, req.Identificador, req.EstruturaId,
            req.ValorPortfolio, req.LombardLimite, req.LombardUtilizado, req.Status), ct);
        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ContaRequest req, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(req.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {req.Moeda}.");
        await mediator.Send(new SaveContaCommand(id, req.Nome, req.Tipo, moeda, req.Saldo,
            req.Instituicao, req.Pais, req.Identificador, req.EstruturaId,
            req.ValorPortfolio, req.LombardLimite, req.LombardUtilizado, req.Status), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteContaCommand(id), ct);
        return NoContent();
    }
}
