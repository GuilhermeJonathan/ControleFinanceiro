using ControleFinanceiro.Application.ReceitasRecorrentes.Commands.CreateReceitaRecorrente;
using ControleFinanceiro.Application.ReceitasRecorrentes.Commands.DeleteReceitaRecorrente;
using ControleFinanceiro.Application.ReceitasRecorrentes.Commands.UpdateReceitaRecorrente;
using ControleFinanceiro.Application.ReceitasRecorrentes.Queries.GetReceitasRecorrentes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReceitasRecorrentesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetReceitasRecorrentesQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReceitaRecorrenteCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReceitaRecorrenteCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteReceitaRecorrenteCommand(id), ct);
        return NoContent();
    }
}
