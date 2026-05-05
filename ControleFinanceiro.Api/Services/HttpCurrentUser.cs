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
            // Se o middleware de família já resolveu, usa o ID efetivo (dono ou próprio)
            if (_accessor.HttpContext?.Items["EffectiveUserId"] is Guid effectiveId)
                return effectiveId;
            // Fallback: lê direto do JWT
            var value = _accessor.HttpContext?.User.FindFirstValue("nameid");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    /// <summary>ID real do usuário logado (sempre o do JWT, nunca o do dono)</summary>
    public Guid RealUserId
    {
        get
        {
            if (_accessor.HttpContext?.Items["RealUserId"] is Guid realId)
                return realId;
            var value = _accessor.HttpContext?.User.FindFirstValue("nameid");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    /// <summary>Nome/email do usuário real — gravado no lançamento para rastrear quem criou</summary>
    public string? RealUserName =>
        _accessor.HttpContext?.User.FindFirstValue("name")
        ?? _accessor.HttpContext?.User.FindFirstValue("unique_name")
        ?? _accessor.HttpContext?.User.FindFirstValue("email");

    /// <summary>Usuário tem permissão para ver e editar todos os imóveis.</summary>
    public bool PodeVerImoveis
    {
        get
        {
            var claim = _accessor.HttpContext?.User.FindFirstValue("podeVerImoveis");
            // Admin (userType=1) também tem acesso global
            var userType = _accessor.HttpContext?.User.FindFirstValue("userType");
            return claim == "true" || userType == "1";
        }
    }
}
