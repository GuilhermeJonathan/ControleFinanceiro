using ControleFinanceiro.Application.Categorias.Commands.CreateCategoria;
using ControleFinanceiro.Application.Categorias.Commands.DeleteCategoria;
using ControleFinanceiro.Application.Categorias.Queries.GetCategorias;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetCategoriasQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoriaCommand(id), ct);
        return NoContent();
    }
}
