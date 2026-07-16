using ControleFinanceiro.Application.Corretores.Commands;
using ControleFinanceiro.Application.Corretores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

/// <summary>
/// Gerenciamento de corretores subordinados e delegação de carteiras.
/// Assessor: gerencia corretores e delega clientes.
/// Corretor: lista seus clientes delegados e aceita convites.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CorretoresController(IMediator mediator) : ControllerBase
{
    // ── Convite ───────────────────────────────────────────────────────────────

    /// <summary>Assessor gera código de convite para um novo corretor.</summary>
    [HttpPost("convite")]
    public async Task<IActionResult> GerarConvite(CancellationToken ct)
    {
        var codigo = await mediator.Send(new GerarConviteCorretorCommand(), ct);
        return Ok(new { codigo });
    }

    /// <summary>Usuário aceita convite e torna-se corretor deste assessor.</summary>
    [HttpPost("aceitar")]
    public async Task<IActionResult> AceitarConvite([FromBody] AceitarConviteCorretorRequest req, CancellationToken ct)
    {
        await mediator.Send(new AceitarConviteCorretorCommand(req.Codigo), ct);
        return NoContent();
    }

    /// <summary>Assessor gera um convite de corretor e envia por e-mail.</summary>
    [HttpPost("convite/email")]
    public async Task<IActionResult> EnviarConviteEmail([FromBody] EnviarConviteCorretorEmailRequest body, CancellationToken ct)
    {
        var codigo = await mediator.Send(new EnviarConviteCorretorEmailCommand(body.Email), ct);
        return Ok(new { codigo });
    }

    /// <summary>Público: valida um código de convite de corretor para a tela /aceitar.</summary>
    [HttpGet("convite/validar/{codigo}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidarConvite(string codigo, CancellationToken ct) =>
        Ok(await mediator.Send(new ValidarConviteCorretorQuery(codigo), ct));

    /// <summary>Público: aceita o convite de corretor pelo link do e-mail, criando/vinculando a conta.</summary>
    [HttpPost("aceitar-publico")]
    [AllowAnonymous]
    public async Task<IActionResult> AceitarPublico([FromBody] AceitarConvitePublicoCorretorCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    // ── Gerenciamento de corretores (assessor) ────────────────────────────────

    /// <summary>Lista todos os corretores do assessor logado.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCorretores(CancellationToken ct) =>
        Ok(await mediator.Send(new GetCorretoresQuery(), ct));

    /// <summary>Assessor revoga acesso de um corretor (e todas as delegações ativas).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RevogarCorretor(Guid id, CancellationToken ct)
    {
        await mediator.Send(new RevogarCorretorCommand(id), ct);
        return NoContent();
    }

    // ── Delegação de carteira ─────────────────────────────────────────────────

    /// <summary>Assessor delega um cliente a um corretor.</summary>
    [HttpPost("delegacoes")]
    public async Task<IActionResult> Delegar([FromBody] DelegarCarteiraRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new DelegarCarteiraCommand(req.CorretorId, req.ClienteId), ct);
        return Ok(new { id });
    }

    /// <summary>Lista todo o histórico de delegações do assessor.</summary>
    [HttpGet("delegacoes")]
    public async Task<IActionResult> GetDelegacoes(CancellationToken ct) =>
        Ok(await mediator.Send(new GetDelegacoesQuery(), ct));

    /// <summary>Assessor revoga delegação específica.</summary>
    [HttpDelete("delegacoes/{id:guid}")]
    public async Task<IActionResult> RevogarDelegacao(Guid id, CancellationToken ct)
    {
        await mediator.Send(new RevogarDelegacaoCommand(id), ct);
        return NoContent();
    }

    // ── Corretor: seus clientes ───────────────────────────────────────────────

    /// <summary>Corretor lista os clientes que foram delegados a ele.</summary>
    [HttpGet("meus-clientes")]
    public async Task<IActionResult> GetClientesDelegados(CancellationToken ct) =>
        Ok(await mediator.Send(new GetClientesDelegadosQuery(), ct));
}

public record AceitarConviteCorretorRequest(string Codigo);
public record DelegarCarteiraRequest(Guid CorretorId, Guid ClienteId);
public record EnviarConviteCorretorEmailRequest(string Email);
