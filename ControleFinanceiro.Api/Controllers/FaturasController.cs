using ControleFinanceiro.Application.Faturas;
using ControleFinanceiro.Application.Faturas.Commands.ImportarFatura;
using ControleFinanceiro.Api.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Route("api/faturas")]
[Authorize]
public class FaturasController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Analisa o Excel da fatura (.xlsx) e retorna as transações extraídas (sem salvar).
    /// </summary>
    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<FaturaTransacaoDto>>> Preview(
        IFormFile arquivo,
        [FromForm] int mesFatura,
        [FromForm] int anoFatura)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest("Arquivo inválido.");

        var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return BadRequest("Formato não suportado. Envie um arquivo .xlsx ou .xls.");

        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms);
        var bytes = ms.ToArray();

        try
        {
            var transacoes = FaturaParser.Parse(bytes, mesFatura, anoFatura);

            if (transacoes.Count == 0)
                return Ok(Array.Empty<FaturaTransacaoDto>());

            return Ok(transacoes);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao processar arquivo: {ex.Message}");
        }
    }

    /// <summary>
    /// Importa as transações confirmadas como lançamentos no cartão.
    /// </summary>
    [HttpPost("importar")]
    public async Task<ActionResult<int>> Importar(
        [FromBody] ImportarFaturaCommand command,
        CancellationToken ct)
    {
        var count = await mediator.Send(command, ct);
        return Ok(count);
    }
}
