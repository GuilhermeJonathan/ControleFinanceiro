using ControleFinanceiro.Application.Vinculos.Commands.AceitarConvite;
using ControleFinanceiro.Application.Vinculos.Commands.GerarConvite;
using ControleFinanceiro.Application.Vinculos.Commands.RemoverVinculo;
using ControleFinanceiro.Application.Vinculos.Queries.GetMeuVinculo;
using ControleFinanceiro.Application.Vinculos.Queries.GetVinculos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VinculosController(IMediator mediator) : ControllerBase
{
    /// <summary>Gera um código de convite para compartilhar os dados com um membro</summary>
    [HttpPost("convite")]
    public async Task<IActionResult> GerarConvite(CancellationToken ct)
    {
        var codigo = await mediator.Send(new GerarConviteCommand(), ct);
        return Ok(new { codigo });
    }

    /// <summary>Aceita um convite (o membro chama este endpoint)</summary>
    [HttpPost("aceitar")]
    public async Task<IActionResult> AceitarConvite([FromBody] AceitarConviteRequest body, CancellationToken ct)
    {
        await mediator.Send(new AceitarConviteCommand(body.Codigo, body.NomeMembro), ct);
        return NoContent();
    }

    /// <summary>Lista os membros vinculados ao usuário atual (visão do dono)</summary>
    [HttpGet]
    public async Task<IActionResult> GetVinculos(CancellationToken ct)
        => Ok(await mediator.Send(new GetVinculosQuery(), ct));

    /// <summary>Verifica se o usuário atual é membro de outra família</summary>
    [HttpGet("meu")]
    public async Task<IActionResult> GetMeuVinculo(CancellationToken ct)
        => Ok(await mediator.Send(new GetMeuVinculoQuery(), ct));

    /// <summary>Remove um vínculo (dono remove membro, ou membro sai)</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await mediator.Send(new RemoverVinculoCommand(id), ct);
        return NoContent();
    }
}

public record AceitarConviteRequest(string Codigo, string NomeMembro);
