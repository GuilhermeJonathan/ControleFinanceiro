using Login.Application.Users.Commands.Authenticate;
using Login.Application.Users.Commands.BlockUser;
using Login.Application.Users.Commands.SetPlan;
using Login.Application.Users.Commands.CreateUser;
using Login.Application.Users.Commands.DeleteUser;
using Login.Application.Users.Commands.RegisterUser;
using Login.Application.Users.Commands.SelfRegisterUser;
using Login.Application.Users.Commands.ResetPassword;
using Login.Application.Users.Commands.ChangePassword;
using Login.Application.Users.Commands.UpdateAvatar;
using Login.Application.Users.Commands.UpdateUser;
using Login.Application.Users.Commands.Refresh;
using Login.Application.Users.Queries.GetUserById;
using Login.Application.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Login.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Auto-cadastro público — sem convite. Inicia trial de 30 dias.</summary>
    [HttpPost("selfregister")]
    [AllowAnonymous]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> SelfRegister(
        [FromBody] SelfRegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Auto-cadastro via convite.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var accessToken = await _mediator.Send(command, cancellationToken);
        return Ok(new { accessToken });
    }

    /// <summary>Autentica o usuário e retorna o token JWT.</summary>
    [HttpPost("authenticate")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Authenticate(
        [FromBody] AuthenticateCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Solicita redefinição de senha (envia e-mail/SMS).</summary>
    [HttpPost("forgotPassword")]
    [AllowAnonymous]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>Valida o hash do link de redefinição de senha.</summary>
    [HttpPost("forgotPassword/validateHash")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateHash(
        [FromBody] ValidateHashCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Valida o código de segurança de redefinição de senha.</summary>
    [HttpPost("forgotPassword/validateSecurityCode")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateSecurityCode(
        [FromBody] ValidateSecurityCodeCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>Redefine a senha via fluxo de recuperação (anônimo).</summary>
    [HttpPut("password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>Redefine a senha do usuário autenticado.</summary>
    [HttpPut("redefinePassword")]
    [Authorize]
    public async Task<IActionResult> RedefinePassword(
        [FromBody] RedefinePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>Verifica se o token JWT ainda é válido.</summary>
    [HttpPost("checkToken")]
    [Authorize]
    public IActionResult CheckToken() => Ok(true);

    /// <summary>Renova o access token usando um refresh token válido.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshCommand(body.RefreshToken), cancellationToken);
        return Ok(result);
    }

    /// <summary>Health check da API.</summary>
    [HttpGet("ok")]
    [AllowAnonymous]
    public IActionResult HealthCheck() => Ok(new { success = true });

    /// <summary>Lista usuários com filtros e paginação.</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Busca um usuário pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Lista usuários de uma empresa específica.</summary>
    [HttpGet("ListByCompany/{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ListByCompany(string id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUsersQuery(null, null, null, null, null, null, null, null, null, null),
            cancellationToken);
        return Ok(result.Items);
    }

    /// <summary>Lista todos os usuários.</summary>
    [HttpGet("ListAllUsers")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ListAllUsers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUsersQuery(null, null, null, null, null, null, null, null, null, null, 1000),
            cancellationToken);
        return Ok(result.Items);
    }

    /// <summary>Cria usuário externo.</summary>
    [HttpPost("external")]
    [Authorize]
    public async Task<IActionResult> CreateExternal(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Cria usuário interno.</summary>
    [HttpPost("internal")]
    [Authorize]
    public async Task<IActionResult> CreateInternal(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>Altera a senha do usuário autenticado.</summary>
    [HttpPatch("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ChangePasswordCommand(body.CurrentPassword, body.NewPassword), cancellationToken);
        return NoContent();
    }

    /// <summary>Atualiza o avatar do usuário autenticado (base64 data URL).</summary>
    [HttpPatch("me/avatar")]
    [Authorize]
    public async Task<IActionResult> UpdateAvatar(
        [FromBody] UpdateAvatarRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateAvatarCommand(body.AvatarUrl), cancellationToken);
        return NoContent();
    }

    /// <summary>Atualiza perfil de um usuário.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Bloqueia ou desbloqueia um usuário.</summary>
    [HttpPatch("{id:guid}/block")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> SetBlock(Guid id, [FromBody] SetBlockRequest body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new BlockUserCommand(id, body.Block), cancellationToken);
        return NoContent();
    }

    /// <summary>Admin: define o plano de um usuário.</summary>
    [HttpPatch("{id:guid}/plan")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> SetPlan(Guid id, [FromBody] SetPlanRequest body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetPlanCommand(id, body.PlanType, body.TrialDays), cancellationToken);
        return NoContent();
    }

    /// <summary>Exclui um usuário e invalida seu token.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return Ok();
    }
}

public record SetBlockRequest(bool Block);
public record SetPlanRequest(int PlanType, int? TrialDays);
public record UpdateAvatarRequest(string? AvatarUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record RefreshRequest(string RefreshToken);
