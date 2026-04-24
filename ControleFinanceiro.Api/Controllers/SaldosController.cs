using ControleFinanceiro.Application.SaldoContas.Commands.CreateConta;
using ControleFinanceiro.Application.SaldoContas.Commands.DeleteConta;
using ControleFinanceiro.Application.SaldoContas.Commands.UpdateConta;
using ControleFinanceiro.Application.SaldoContas.Commands.UpsertSaldo;
using ControleFinanceiro.Application.SaldoContas.Queries.GetSaldos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SaldosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetSaldosQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContaCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContaCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteContaCommand(id), ct);
        return NoContent();
    }

    // Mantido para compatibilidade legada
    [HttpPut("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertSaldoCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { id });
    }
}
