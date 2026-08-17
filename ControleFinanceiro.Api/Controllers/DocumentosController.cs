using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.Documentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetDocumentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetTarefasDocumento;
using ControleFinanceiro.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

/// <summary>
/// Documentos anexados (vault). O backend faz proxy do upload para o Supabase Storage.
/// Alvo: 1=Cliente, 2=Ativo, 3=Estrutura.
/// </summary>
[ApiController]
[Authorize]
[Route("api/documentos")]
public class DocumentosController(IMediator mediator, IArquivoStorage storage) : ControllerBase
{
    private const long LimiteBytes = 50L * 1024 * 1024; // 50 MB (limite do bucket)

    /// <summary>Lista os documentos de um alvo. Ex.: GET /api/documentos?alvo=2&amp;alvoId={ativoId}</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int alvo, [FromQuery] Guid? alvoId, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(AlvoDocumento), alvo)) return BadRequest("Alvo inválido.");
        var docs = await mediator.Send(new GetDocumentosQuery((AlvoDocumento)alvo, alvoId), ct);
        return Ok(docs);
    }

    /// <summary>Upload (multipart/form-data): arquivo + alvo + alvoId? + categoria?.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(LimiteBytes)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile arquivo,
        [FromForm] int alvo,
        [FromForm] Guid? alvoId,
        [FromForm] string? categoria,
        CancellationToken ct)
    {
        if (!storage.Configurado) return StatusCode(503, "Armazenamento de arquivos não configurado no servidor.");
        if (arquivo is null || arquivo.Length == 0) return BadRequest("Arquivo vazio.");
        if (arquivo.Length > LimiteBytes) return BadRequest("Arquivo excede o limite de 50 MB.");
        if (!Enum.IsDefined(typeof(AlvoDocumento), alvo)) return BadRequest("Alvo inválido.");

        await using var stream = arquivo.OpenReadStream();
        var id = await mediator.Send(new UploadDocumentoCommand(
            (AlvoDocumento)alvo, alvoId, arquivo.FileName, arquivo.ContentType, arquivo.Length, stream, categoria), ct);
        return Ok(new { id });
    }

    /// <summary>Baixa o binário do documento (o backend puxa do storage e devolve).</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var meta = await mediator.Send(new GetDocumentoParaDownloadQuery(id), ct);
        if (meta is null) return NotFound();
        if (!storage.Configurado) return StatusCode(503, "Armazenamento não configurado.");

        var stream = await storage.DownloadAsync(meta.Value.StoragePath, ct);
        return File(stream, meta.Value.ContentType ?? "application/octet-stream", meta.Value.Nome);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteDocumentoCommand(id), ct);
        return NoContent();
    }

    // ── Tarefas de documento (assessor pede → cliente anexa) ────────────────────

    public record CriarTarefaRequest(Guid ClienteId, string Titulo, string? Descricao, string? AtalhoRota);

    /// <summary>Tarefas de documento recebidas pelo cliente logado.</summary>
    [HttpGet("tarefas")]
    public async Task<IActionResult> MinhasTarefas(CancellationToken ct) =>
        Ok(await mediator.Send(new GetTarefasDocumentoClienteQuery(), ct));

    /// <summary>Tarefas que o assessor criou para um cliente.</summary>
    [HttpGet("tarefas/cliente/{clienteId:guid}")]
    public async Task<IActionResult> TarefasDoCliente(Guid clienteId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetTarefasDocumentoAssessorQuery(clienteId), ct));

    /// <summary>Assessor cria uma tarefa ("adicione o documento X") para o cliente.</summary>
    [HttpPost("tarefas")]
    public async Task<IActionResult> CriarTarefa([FromBody] CriarTarefaRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CriarTarefaDocumentoCommand(req.ClienteId, req.Titulo, req.Descricao, req.AtalhoRota), ct);
        return Ok(new { id });
    }

    /// <summary>Cliente marca a tarefa como concluída (após anexar o documento).</summary>
    [HttpPatch("tarefas/{id:guid}/concluir")]
    public async Task<IActionResult> ConcluirTarefa(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ConcluirTarefaDocumentoCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("tarefas/{id:guid}")]
    public async Task<IActionResult> ExcluirTarefa(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTarefaDocumentoCommand(id), ct);
        return NoContent();
    }
}
