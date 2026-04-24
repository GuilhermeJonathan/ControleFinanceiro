using ControleFinanceiro.Application.Horas.Commands.CreateHoras;
using ControleFinanceiro.Application.Horas.Commands.DeleteHoras;
using ControleFinanceiro.Application.Horas.Commands.UpdateHoras;
using ControleFinanceiro.Application.Horas.Queries.GetHorasByMes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HorasController(IMediator mediator) : ControllerBase
{
    [HttpGet("{mes}/{ano}")]
    public async Task<IActionResult> GetByMes(int mes, int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetHorasByMesQuery(mes, ano), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHorasCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHorasCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteHorasCommand(id), ct);
        return NoContent();
    }
}
