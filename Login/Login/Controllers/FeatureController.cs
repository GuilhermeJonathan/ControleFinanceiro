using Login.Application.Features.Queries.GetNpsAccess;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FeatureController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeatureController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Verifica acesso à feature NPS.</summary>
    [HttpGet("searchNPS")]
    public async Task<IActionResult> SearchNps(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNpsAccessQuery(), cancellationToken);
        return Ok(result);
    }
}
