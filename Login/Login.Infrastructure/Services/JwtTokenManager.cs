using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;  // mantido para Claim()
using System.Text;
using Login.Application.Common.Interfaces;
using Login.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Login.Infrastructure.Services;

public class JwtTokenManager : ITokenManager
{
    private readonly IConfiguration _configuration;
    private static readonly HashSet<Guid> _invalidatedTokens = new();

    public JwtTokenManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Generate(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiresInMinutes = int.Parse(jwtSettings["ExpiresInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userType", ((int)user.UserTypeId).ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool Validate(string token)
    {
        // Validação via middleware JWT do ASP.NET Core
        return true;
    }

    public void Invalidate(Guid userId)
    {
        _invalidatedTokens.Add(userId);
        // Em produção: persistir lista de tokens inválidos no Redis/banco
    }
}
