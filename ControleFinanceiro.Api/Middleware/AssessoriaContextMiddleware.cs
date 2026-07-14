using System.Security.Claims;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Api.Middleware;

/// <summary>
/// Modo "visualizar como" do assessor. Quando a requisição traz o header
/// X-Assessoria-Cliente com o id de um cliente vinculado:
///   1. Exige perfil Assessor (userType=3) ou Admin (userType=1) no JWT;
///   2. Exige vínculo de assessoria ATIVO (aceito e não revogado);
///   3. Permite SOMENTE métodos de leitura (GET/HEAD/OPTIONS) — escrita retorna 403.
///      O assessor apenas VISUALIZA os dados do cliente; nunca altera.
///   4. Sobrescreve EffectiveUserId com o id do cliente (RealUserId segue o assessor).
/// Deve ser registrado DEPOIS do FamiliaContextMiddleware para ter precedência.
/// </summary>
public class AssessoriaContextMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Assessoria-Cliente";

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.Request.Headers.TryGetValue(HeaderName, out var headerValue))
        {
            if (!Guid.TryParse(headerValue, out var clienteId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Header X-Assessoria-Cliente inválido." });
                return;
            }

            var userType = context.User.FindFirstValue("userType");
            if (userType is not ("3" or "1"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Apenas assessores podem visualizar dados de clientes." });
                return;
            }

            var rawId = context.User.FindFirstValue("nameid");
            if (!Guid.TryParse(rawId, out var assessorId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Modo view-as é estritamente leitura — o servidor garante, não a UI
            if (!HttpMethods.IsGet(context.Request.Method) &&
                !HttpMethods.IsHead(context.Request.Method) &&
                !HttpMethods.IsOptions(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Modo visualização é somente leitura." });
                return;
            }

            var vinculoAtivo = await db.VinculosAssessoria.AnyAsync(v =>
                v.AssessorId == assessorId &&
                v.ClienteId == clienteId &&
                v.AceitoEm != null &&
                v.RevogadoEm == null);

            if (!vinculoAtivo)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Vínculo de assessoria não encontrado ou revogado." });
                return;
            }

            context.Items["EffectiveUserId"] = clienteId;
            context.Items["RealUserId"] = assessorId;
        }

        await next(context);
    }
}
