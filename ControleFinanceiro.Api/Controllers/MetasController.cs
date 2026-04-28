using ControleFinanceiro.Application.Metas.Commands.AtualizarValorMeta;
using ControleFinanceiro.Application.Metas.Commands.CreateMeta;
using ControleFinanceiro.Application.Metas.Commands.DeleteMeta;
using ControleFinanceiro.Application.Metas.Commands.UpdateMeta;
using ControleFinanceiro.Application.Metas.Queries.GetMetas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MetasController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetMetasQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMetaCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMetaCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/valor")]
    public async Task<IActionResult> AtualizarValor(Guid id, [FromBody] AtualizarValorMetaRequest body, CancellationToken ct)
    {
        await mediator.Send(new AtualizarValorMetaCommand(id, body.NovoValor), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMetaCommand(id), ct);
        return NoContent();
    }
}

public record AtualizarValorMetaRequest(decimal NovoValor);
