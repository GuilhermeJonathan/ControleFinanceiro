using Login.Application.Invites.Commands.CreateInvite;
using Login.Application.Invites.Queries.ListInvites;
using Login.Application.Invites.Queries.ValidateInvite;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers;

[ApiController]
[Route("[controller]")]
public class InviteController : ControllerBase
{
    private readonly IMediator _mediator;

    public InviteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gera um novo convite de auto-cadastro.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateInviteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        var appUrl = Environment.GetEnvironmentVariable("APP_URL") ?? "https://app.findog.com.br";
        var link = $"{appUrl}/register?invite={result.Token}";
        return Ok(new { result.Token, result.ExpiresAt, link });
    }

    /// <summary>Lista os convites gerados pelo usuário autenticado.</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ListMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListInvitesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Valida um convite pelo token.</summary>
    [HttpGet("{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> Validate(string token, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ValidateInviteQuery(token), cancellationToken);
        return Ok(result);
    }
}
