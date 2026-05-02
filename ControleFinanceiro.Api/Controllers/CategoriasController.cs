using ControleFinanceiro.Application.Categorias.Commands.AtualizarLimiteCategoria;
using ControleFinanceiro.Application.Categorias.Commands.CreateCategoria;
using ControleFinanceiro.Application.Categorias.Commands.DeleteCategoria;
using ControleFinanceiro.Application.Categorias.Queries.GetCategorias;
using ControleFinanceiro.Application.Categorias.Queries.GetOrcamento;
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
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetCategoriasQuery(page, pageSize), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPatch("{id}/limite")]
    public async Task<IActionResult> AtualizarLimite(Guid id, [FromBody] AtualizarLimiteCategoriaCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpGet("orcamento/{mes}/{ano}")]
    public async Task<IActionResult> GetOrcamento(int mes, int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetOrcamentoQuery(mes, ano), ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoriaCommand(id), ct);
        return NoContent();
    }
}
