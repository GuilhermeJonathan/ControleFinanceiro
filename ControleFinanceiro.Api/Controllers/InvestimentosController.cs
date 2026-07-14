using ControleFinanceiro.Application.Patrimonio.Commands.CreateInvestimento;
using ControleFinanceiro.Application.Patrimonio.Commands.DeleteInvestimento;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdateInvestimento;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

public record InvestimentoRequest(
    string Nome,
    TipoInvestimento Tipo,
    string Moeda,
    string? Corretora,
    string? Ticker,
    decimal ValorAplicado,
    decimal ValorAtual,
    decimal? RentabilidadeAnualPct);

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InvestimentosController(IMediator mediator) : ControllerBase
{
    private static readonly Dictionary<string, MoedaPatrimonio> MoedaMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BRL"] = MoedaPatrimonio.BRL,
        ["USD"] = MoedaPatrimonio.USD,
        ["EUR"] = MoedaPatrimonio.EUR,
        ["CHF"] = MoedaPatrimonio.CHF,
        ["GBP"] = MoedaPatrimonio.GBP,
    };

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo(CancellationToken ct) =>
        Ok(await mediator.Send(new GetResumoInvestimentosQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvestimentoRequest request, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda invalida: {request.Moeda}.");

        var id = await mediator.Send(
            new CreateInvestimentoCommand(request.Nome, request.Tipo, moeda, request.Corretora,
                request.Ticker, request.ValorAplicado, request.ValorAtual, request.RentabilidadeAnualPct), ct);

        return CreatedAtAction(nameof(GetResumo), new { }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] InvestimentoRequest request, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda invalida: {request.Moeda}.");

        await mediator.Send(
            new UpdateInvestimentoCommand(id, request.Nome, request.Tipo, moeda, request.Corretora,
                request.Ticker, request.ValorAplicado, request.ValorAtual, request.RentabilidadeAnualPct), ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteInvestimentoCommand(id), ct);
        return NoContent();
    }
}
