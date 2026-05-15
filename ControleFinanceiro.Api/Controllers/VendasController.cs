using ControleFinanceiro.Application.Vendas.Commands.AtualizarStatusVenda;
using ControleFinanceiro.Application.Vendas.Commands.CreateVenda;
using ControleFinanceiro.Application.Vendas.Commands.DeleteVenda;
using ControleFinanceiro.Application.Vendas.Commands.UpdateVenda;
using ControleFinanceiro.Application.Vendas.Queries.GetResumoVendas;
using ControleFinanceiro.Application.Vendas.Queries.GetVendas;
using ControleFinanceiro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendasController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? de,
        [FromQuery] DateTime? ate,
        [FromQuery] Guid? produtoId,
        [FromQuery] StatusVenda? status,
        CancellationToken ct)
        => Ok(await mediator.Send(new GetVendasQuery(de, ate, produtoId, status), ct));

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo(CancellationToken ct)
        => Ok(await mediator.Send(new GetResumoVendasQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendaCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendaCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarStatusVendaRequest body, CancellationToken ct)
    {
        await mediator.Send(new AtualizarStatusVendaCommand(id, body.Status), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteVendaCommand(id), ct);
        return NoContent();
    }
}

public record AtualizarStatusVendaRequest(StatusVenda Status);
