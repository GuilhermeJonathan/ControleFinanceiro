using System.Text;
using ControleFinanceiro.Application;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Api.BackgroundServices;
using ControleFinanceiro.Api.Extensions;
using ControleFinanceiro.Api.Middleware;
using ControleFinanceiro.Api.Services;
using ControleFinanceiro.Infrastructure;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

// JWT — mesma SecretKey/Issuer/Audience do Login API
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey não configurado.");
if (string.IsNullOrWhiteSpace(secretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey não configurado. Defina via variável de ambiente (JwtSettings__SecretKey).");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Mantém os nomes curtos do JWT (nameid, unique_name, email) sem remapear para URLs longos
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
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<DailyJobService>();
builder.Services.AddWhatsApp();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<FamiliaContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
