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

        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userType", ((int)user.UserTypeId).ToString()),
            new Claim("podeVerImoveis", user.PodeVerImoveis ? "true" : "false"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // iat explícito para comparação com TokenRevokedAt
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool Validate(string token) => true; // validação feita pelo middleware JWT

    public void Invalidate(Guid userId) { } // revogação via User.RevokeTokens() + DB
}
