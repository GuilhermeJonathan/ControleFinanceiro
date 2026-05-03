using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Login.Application.Common.Interfaces;

namespace Login.Infrastructure;

public class HttpUserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            // JWT grava como "nameid" (JwtRegisteredClaimNames.NameId)
            var value = user?.FindFirstValue(JwtRegisteredClaimNames.NameId)
                     ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? string.Empty;

    public string UserType
        => _httpContextAccessor.HttpContext?.User.FindFirstValue("userType") ?? string.Empty;
}
