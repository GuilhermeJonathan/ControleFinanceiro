using ControleFinanceiro.Api.ExtratoParser;
using ControleFinanceiro.Application.Lancamentos.Commands.ImportarExtrato;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExtratoController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Parse do OFX — retorna preview das transações sem salvar.
    /// </summary>
    [HttpPost("parse")]
    public async Task<IActionResult> Parse(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Arquivo OFX não enviado.");

        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
        var content = await reader.ReadToEndAsync(ct);

        var transacoes = OfxParser.Parse(content);
        if (transacoes.Count == 0)
            return BadRequest("Nenhuma transação encontrada no arquivo OFX.");

        // Retorna preview para o frontend mostrar antes de confirmar
        var preview = transacoes.Select(t => new
        {
            id          = t.Id,
            descricao   = t.Memo,
            valor       = t.Valor,
            data        = t.Data,
            mes         = t.Data.Month,
            ano         = t.Data.Year,
            tipo        = t.Valor >= 0 ? "Credito" : "Debito",
            categoriaNome = (string?)null,
        }).ToList();

        return Ok(preview);
    }

    /// <summary>
    /// Importa as transações confirmadas pelo usuário.
    /// </summary>
    [HttpPost("importar")]
    public async Task<IActionResult> Importar(
        [FromBody] ImportarExtratoCommand command, CancellationToken ct)
    {
        var count = await mediator.Send(command, ct);
        return Ok(new { importados = count });
    }
}
