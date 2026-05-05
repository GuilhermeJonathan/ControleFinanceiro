using ControleFinanceiro.Application.Imoveis.Commands.AddComentario;
using ControleFinanceiro.Application.Imoveis.Commands.AddFoto;
using ControleFinanceiro.Application.Imoveis.Commands.CreateImovel;
using ControleFinanceiro.Application.Imoveis.Commands.DeleteImovel;
using ControleFinanceiro.Application.Imoveis.Commands.RemoveComentario;
using ControleFinanceiro.Application.Imoveis.Commands.RemoveFoto;
using ControleFinanceiro.Application.Imoveis.Commands.UpdateImovel;
using ControleFinanceiro.Application.Imoveis.Queries.GetImovelById;
using ControleFinanceiro.Application.Imoveis.Queries.GetImoveis;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ImoveisController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetImoveisQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new GetImovelByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateImovelCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateImovelCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteImovelCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/fotos")]
    public async Task<IActionResult> AddFoto(Guid id, [FromBody] AddFotoRequest body, CancellationToken ct)
    {
        var fotoId = await mediator.Send(new AddFotoCommand(id, body.Dados, body.Ordem), ct);
        return Created(string.Empty, new { id = fotoId });
    }

    [HttpDelete("fotos/{fotoId:guid}")]
    public async Task<IActionResult> RemoveFoto(Guid fotoId, CancellationToken ct)
    {
        await mediator.Send(new RemoveFotoCommand(fotoId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/comentarios")]
    public async Task<IActionResult> AddComentario(Guid id, [FromBody] AddComentarioRequest body, CancellationToken ct)
    {
        var comentarioId = await mediator.Send(new AddComentarioCommand(id, body.Texto), ct);
        return Created(string.Empty, new { id = comentarioId });
    }

    [HttpDelete("comentarios/{comentarioId:guid}")]
    public async Task<IActionResult> RemoveComentario(Guid comentarioId, CancellationToken ct)
    {
        await mediator.Send(new RemoveComentarioCommand(comentarioId), ct);
        return NoContent();
    }
}

public record AddFotoRequest(string Dados, int Ordem);
public record AddComentarioRequest(string Texto);
