using ControleFinanceiro.Application.Produtos.Commands.CreateProduto;
using ControleFinanceiro.Application.Produtos.Commands.DeleteProduto;
using ControleFinanceiro.Application.Produtos.Commands.UpdateProduto;
using ControleFinanceiro.Application.Produtos.Queries.GetProdutos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProdutosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetProdutosQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProdutoCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProdutoCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProdutoCommand(id), ct);
        return NoContent();
    }
}
