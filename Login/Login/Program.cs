using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Login.Application;
using Login.Application.Common.Interfaces;
using Login.Extensions;
using Login.Infrastructure;
using Login.Infrastructure.Persistence;
using Login.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Npgsql: permite DateTime (em vez de DateTimeOffset) para colunas timestamptz
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
    o.Debug = builder.Environment.IsDevelopment();
    o.TracesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.2;
    o.MinimumBreadcrumbLevel = Microsoft.Extensions.Logging.LogLevel.Information;
    o.MinimumEventLevel = Microsoft.Extensions.Logging.LogLevel.Error;
    o.SendDefaultPii = false; // não envia dados pessoais
});

builder.Services.AddLoginRateLimiting();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "controlefinanceiro.security.api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey não configurado.");
if (string.IsNullOrWhiteSpace(secretKey) || secretKey == "YOUR_SECRET_KEY_HERE")
    throw new InvalidOperationException("JwtSettings:SecretKey está com valor padrão inseguro. Configure uma chave real via variável de ambiente (JwtSettings__SecretKey).");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var nameId = context.Principal?.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (!Guid.TryParse(nameId, out var userId)) { context.Fail("Token inválido."); return; }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var cacheKey = $"usr_status_{userId}";

            if (!cache.TryGetValue(cacheKey, out (bool IsActive, bool IsBlocked, DateTime? TokenRevokedAt) status))
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var row = await db.Users.AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.IsActive, u.IsBlocked, u.TokenRevokedAt })
                    .FirstOrDefaultAsync();

                if (row is null) { context.Fail("Usuário não encontrado."); return; }

                status = (row.IsActive, row.IsBlocked, row.TokenRevokedAt);
                cache.Set(cacheKey, status, TimeSpan.FromMinutes(2));
            }

            if (!status.IsActive || status.IsBlocked)
            { context.Fail("Usuário inativo ou bloqueado."); return; }

            if (status.TokenRevokedAt.HasValue)
            {
                var iatClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
                if (long.TryParse(iatClaim, out var iatUnix))
                {
                    var tokenIssuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix).UtcDateTime;
                    if (tokenIssuedAt < status.TokenRevokedAt.Value)
                    { context.Fail("Token revogado."); return; }
                }
            }
        }
    };
});

// Política Admin: exige claim userType = 1
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("userType", "1"));
});
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// CORS: permite apenas origens conhecidas em produção
var allowedOrigins = new[]
{
    "https://app.findog.com.br",
    "https://www.findog.com.br",
    "https://findog.com.br",
    "https://financeiro-web-two.vercel.app"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

// Camadas CQRS + Clean Architecture
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Login.Infrastructure.Services.TrialExpirationEmailService>();
builder.Services.AddHostedService<Login.Infrastructure.Services.ReengagementEmailService>();
builder.Services.AddHostedService<Login.Infrastructure.Services.WarmupService>();

// IUserAccessor — resolve via HttpContext
builder.Services.AddScoped<IUserAccessor, Login.Infrastructure.HttpUserAccessor>();

var app = builder.Build();

// Aplica migrations pendentes antes de aceitar tráfego e antes dos hosted services
using (var migrationScope = app.Services.CreateScope())
{
    var db = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "controlefinanceiro.security.api v1"));
}

// Preflight CORS — deixa o middleware de CORS do ASP.NET Core gerenciar (não hardcode "*")


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
