using Login.Application.Profiles.Commands.CreateProfile;
using Login.Application.Profiles.Commands.DeleteProfile;
using Login.Application.Profiles.Queries.GetProfiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lista perfis com filtros.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? Type_Id,
        [FromQuery] string? Name,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProfilesQuery(Type_Id, Name), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lista perfis por tipo de usuário.</summary>
    [HttpGet("UserType/{id:int}")]
    public async Task<IActionResult> GetByUserType(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProfilesQuery(id, null), cancellationToken);
        return Ok(result);
    }

    /// <summary>Cria um novo perfil.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id }, null);
    }

    /// <summary>Desativa um perfil.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProfileCommand(id), cancellationToken);
        return NoContent();
    }
}
