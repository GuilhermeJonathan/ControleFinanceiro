using System.Security.Claims;
using ControleFinanceiro.Application.Common.Interfaces;

namespace ControleFinanceiro.Api.Services;

public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _accessor.HttpContext?.User.FindFirstValue("nameid");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
