using ControleFinanceiro.Application.Cartoes.Commands.CreateCartao;
using ControleFinanceiro.Application.Cartoes.Commands.UpdateCartao;
using ControleFinanceiro.Application.Cartoes.Commands.CreateParcela;
using ControleFinanceiro.Application.Cartoes.Commands.DeleteCartao;
using ControleFinanceiro.Application.Cartoes.Commands.DeleteParcela;
using ControleFinanceiro.Application.Cartoes.Commands.UpdateParcela;
using ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartoesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int mes, [FromQuery] int ano, CancellationToken ct)
    {
        if (mes == 0) mes = DateTime.Now.Month;
        if (ano == 0) ano = DateTime.Now.Year;
        return Ok(await mediator.Send(new GetCartoesQuery(mes, ano), ct));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCartao([FromBody] CreateCartaoCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCartao(Guid id, [FromBody] UpdateCartaoCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartao(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCartaoCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{cartaoId}/parcelas")]
    public async Task<IActionResult> CreateParcela(Guid cartaoId, [FromBody] CreateParcelaCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command with { CartaoCreditoId = cartaoId }, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{cartaoId}/parcelas/{parcelaId}")]
    public async Task<IActionResult> UpdateParcela(Guid parcelaId, [FromBody] UpdateParcelaCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = parcelaId }, ct);
        return NoContent();
    }

    [HttpDelete("{cartaoId}/parcelas/{parcelaId}")]
    public async Task<IActionResult> DeleteParcela(Guid parcelaId, CancellationToken ct)
    {
        await mediator.Send(new DeleteParcelaCommand(parcelaId), ct);
        return NoContent();
    }
}
