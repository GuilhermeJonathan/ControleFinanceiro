using Login.Application.Notifications.Commands.SendEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers;

/// <summary>
/// Endpoints internos server-to-server (protegidos por service key), consumidos por
/// outras APIs do ecossistema. Nunca devem ser chamados diretamente pelo cliente.
/// </summary>
[ApiController]
[Route("internal")]
public class InternalController(IMediator mediator, IConfiguration configuration) : ControllerBase
{
    private bool ServiceKeyValida(string? serviceKey)
    {
        var expected = configuration["ServiceAuth:Key"];
        return !string.IsNullOrWhiteSpace(expected) && serviceKey == expected;
    }

    /// <summary>Gateway central de e-mail: envia um e-mail já montado (assunto + HTML).</summary>
    [HttpPost("email")]
    [AllowAnonymous]
    public async Task<IActionResult> SendEmail(
        [FromBody] SendEmailCommand command,
        [FromHeader(Name = "X-Service-Key")] string? serviceKey,
        CancellationToken cancellationToken)
    {
        if (!ServiceKeyValida(serviceKey)) return Unauthorized();

        await mediator.Send(command, cancellationToken);
        return Ok();
    }
}
