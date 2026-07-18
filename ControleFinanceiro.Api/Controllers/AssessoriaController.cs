using ControleFinanceiro.Application.Assessoria.Commands.AceitarConviteAssessoria;
using ControleFinanceiro.Application.Assessoria.Commands.AceitarConvitePublico;
using ControleFinanceiro.Application.Assessoria.Commands.CriarRecomendacao;
using ControleFinanceiro.Application.Assessoria.Commands.MarcarRespostasVistas;
using ControleFinanceiro.Application.Assessoria.Commands.ReenviarConvite;
using ControleFinanceiro.Application.Assessoria.Commands.EnviarConviteEmail;
using ControleFinanceiro.Application.Assessoria.Commands.ExcluirRecomendacao;
using ControleFinanceiro.Application.Assessoria.Commands.GerarConviteAssessoria;
using ControleFinanceiro.Application.Assessoria.Commands.ResponderRecomendacao;
using ControleFinanceiro.Application.Assessoria.Commands.RevogarVinculoAssessoria;
using ControleFinanceiro.Application.Assessoria.Queries.GetAnaliseIa;
using ControleFinanceiro.Application.Assessoria.Queries.GetClientesAssessoria;
using ControleFinanceiro.Application.Assessoria.Queries.GetConvitesHistorico;
using ControleFinanceiro.Application.Assessoria.Queries.GetMeuAssessor;
using ControleFinanceiro.Application.Assessoria.Queries.GetRecomendacoes;
using ControleFinanceiro.Application.Assessoria.Queries.GetRespostasRecomendacoes;
using ControleFinanceiro.Application.Assessoria.Queries.GetSaudeFinanceira;
using ControleFinanceiro.Application.Assessoria.Queries.ValidarConvite;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AssessoriaController(IMediator mediator) : ControllerBase
{
    /// <summary>Assessor: gera um código de convite para um novo cliente.</summary>
    [HttpPost("convite")]
    public async Task<IActionResult> GerarConvite(CancellationToken cancellationToken)
    {
        var codigo = await mediator.Send(new GerarConviteAssessoriaCommand(), cancellationToken);
        return Ok(new { codigo });
    }

    /// <summary>Cliente: aceita o convite de um assessor.</summary>
    [HttpPost("aceitar")]
    public async Task<IActionResult> Aceitar([FromBody] AceitarConviteAssessoriaRequest body, CancellationToken cancellationToken)
    {
        await mediator.Send(new AceitarConviteAssessoriaCommand(body.Codigo, body.NomeCliente), cancellationToken);
        return NoContent();
    }

    /// <summary>Assessor reenvia o e-mail de um convite de cliente pendente (renova a validade).</summary>
    [HttpPost("convite/{id:guid}/reenviar")]
    public async Task<IActionResult> ReenviarConvite(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new ReenviarConviteEmailCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Público: valida um código de convite para a tela /aceitar (sem login).</summary>
    [HttpGet("convite/validar/{codigo}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidarConvite(string codigo, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ValidarConviteAssessoriaQuery(codigo), cancellationToken));

    /// <summary>Público: aceita o convite pelo link do e-mail, criando/vinculando a conta do cliente.</summary>
    [HttpPost("aceitar-publico")]
    [AllowAnonymous]
    public async Task<IActionResult> AceitarPublico([FromBody] AceitarConvitePublicoAssessoriaCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Assessor: gera um convite e envia por e-mail ao cliente.</summary>
    [HttpPost("convite/email")]
    public async Task<IActionResult> EnviarConviteEmail([FromBody] EnviarConviteEmailRequest body, CancellationToken cancellationToken)
    {
        var codigo = await mediator.Send(new EnviarConviteEmailCommand(body.Email), cancellationToken);
        return Ok(new { codigo });
    }

    /// <summary>Assessor: histórico completo de convites (pendentes, aceitos e revogados).</summary>
    [HttpGet("convites/historico")]
    public async Task<IActionResult> GetConvitesHistorico(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetConvitesHistoricoQuery(), cancellationToken));

    /// <summary>Assessor: lista os clientes da carteira (pendentes e ativos).</summary>
    [HttpGet("clientes")]
    public async Task<IActionResult> GetClientes(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetClientesAssessoriaQuery(), cancellationToken));

    /// <summary>Cliente: consulta quem o assessora.</summary>
    [HttpGet("meu-assessor")]
    public async Task<IActionResult> GetMeuAssessor(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetMeuAssessorQuery(), cancellationToken));

    /// <summary>
    /// Score de saúde financeira do usuário efetivo. Sob o header X-Assessoria-Cliente,
    /// retorna o score do cliente visualizado.
    /// </summary>
    [HttpGet("saude/{mes:int}/{ano:int}")]
    public async Task<IActionResult> GetSaude(int mes, int ano, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSaudeFinanceiraQuery(mes, ano), cancellationToken));

    /// <summary>Assessor ou cliente: revoga o vínculo.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revogar(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RevogarVinculoAssessoriaCommand(id), cancellationToken);
        return NoContent();
    }

    // ── Recomendações (F3) ───────────────────────────────────────────────────

    /// <summary>Assessor: cria uma recomendação para um cliente da carteira.</summary>
    [HttpPost("recomendacoes")]
    public async Task<IActionResult> CriarRecomendacao([FromBody] CriarRecomendacaoRequest body, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CriarRecomendacaoCommand(body.ClienteId, body.Tipo, body.Texto, body.CategoriaId), cancellationToken);
        return Ok(new { id });
    }

    /// <summary>Cliente: aceita ou recusa uma recomendação.</summary>
    [HttpPatch("recomendacoes/{id:guid}/responder")]
    public async Task<IActionResult> ResponderRecomendacao(Guid id, [FromBody] ResponderRecomendacaoRequest body, CancellationToken cancellationToken)
    {
        await mediator.Send(new ResponderRecomendacaoCommand(id, body.Aceitar, body.Comentario), cancellationToken);
        return NoContent();
    }

    /// <summary>Assessor: exclui uma recomendação ainda pendente.</summary>
    [HttpDelete("recomendacoes/{id:guid}")]
    public async Task<IActionResult> ExcluirRecomendacao(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new ExcluirRecomendacaoCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Cliente: lista as recomendações recebidas.</summary>
    [HttpGet("recomendacoes")]
    public async Task<IActionResult> GetRecomendacoesCliente(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRecomendacoesClienteQuery(), cancellationToken));

    /// <summary>Assessor: lista as recomendações enviadas a um cliente.</summary>
    [HttpGet("recomendacoes/cliente/{clienteId:guid}")]
    public async Task<IActionResult> GetRecomendacoesAssessor(Guid clienteId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRecomendacoesAssessorQuery(clienteId), cancellationToken));

    /// <summary>Assessor: respostas dos clientes às recomendações (sino de notificações).</summary>
    [HttpGet("recomendacoes/respostas")]
    public async Task<IActionResult> GetRespostas(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRespostasRecomendacoesQuery(), cancellationToken));

    /// <summary>Assessor: marca todas as respostas como vistas (zera o badge do sino).</summary>
    [HttpPost("recomendacoes/respostas/marcar-vistas")]
    public async Task<IActionResult> MarcarRespostasVistas(CancellationToken cancellationToken)
    {
        await mediator.Send(new MarcarRespostasVistasCommand(), cancellationToken);
        return NoContent();
    }

    // ── Análise com IA (F4) ──────────────────────────────────────────────────

    /// <summary>
    /// Rascunho de análise gerado por IA para o usuário efetivo (sob view-as, o cliente).
    /// O assessor edita o texto antes de enviar como recomendação.
    /// </summary>
    [HttpGet("analise-ia/{mes:int}/{ano:int}")]
    public async Task<IActionResult> GetAnaliseIa(int mes, int ano, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetAnaliseIaQuery(mes, ano), cancellationToken));
}

public record AceitarConviteAssessoriaRequest(string Codigo, string NomeCliente);
public record EnviarConviteEmailRequest(string Email);
public record CriarRecomendacaoRequest(Guid ClienteId, int Tipo, string Texto, Guid? CategoriaId);
public record ResponderRecomendacaoRequest(bool Aceitar, string? Comentario);
