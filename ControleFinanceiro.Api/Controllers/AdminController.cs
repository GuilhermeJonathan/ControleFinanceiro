using ControleFinanceiro.Application.Admin.Queries.GetAdminOverview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

/// <summary>Painel do admin da plataforma (acima dos assessores). Acesso restrito a userType=1.</summary>
[ApiController]
[Authorize]
[Route("api/admin")]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary>Visão consolidada: totais da plataforma + lista de assessorias.</summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAdminOverviewQuery(), ct));
}
