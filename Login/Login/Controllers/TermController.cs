using Login.Application.Terms.Commands.AcceptTerm;
using Login.Application.Terms.Queries.CheckTermAccepted;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TermController : ControllerBase
{
    private readonly IMediator _mediator;

    public TermController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Verifica se o usuário já aceitou um termo.</summary>
    [HttpGet("{term}/accepted")]
    public async Task<IActionResult> IsAccepted(string term, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckTermAcceptedQuery(term), cancellationToken);
        return Ok(result);
    }

    /// <summary>Registra o aceite de um termo pelo usuário logado.</summary>
    [HttpPost("{term}/accept")]
    public async Task<IActionResult> Accept(string term, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        await _mediator.Send(new AcceptTermCommand(term, ipAddress, userAgent), cancellationToken);
        return Ok();
    }
}
