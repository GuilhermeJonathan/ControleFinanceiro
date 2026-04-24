using Login.Application.Integration.Commands.Authorize;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers;

[ApiController]
[Route("[controller]")]
public class IntegrationController : ControllerBase
{
    private readonly IMediator _mediator;

    public IntegrationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Autenticação machine-to-machine (M2M).</summary>
    [HttpPost("authorize")]
    [AllowAnonymous]
    public async Task<IActionResult> Authorize(
        [FromBody] AuthorizeIntegrationCommand command,
        CancellationToken cancellationToken)
    {
        var token = await _mediator.Send(command, cancellationToken);
        return Ok(new { accessToken = token });
    }
}
