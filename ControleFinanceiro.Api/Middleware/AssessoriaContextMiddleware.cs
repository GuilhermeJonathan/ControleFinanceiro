using System.Security.Claims;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Api.Middleware;

/// <summary>
/// Modo "visualizar como" do assessor/corretor.
/// Quando a requisição traz X-Assessoria-Cliente:
///   - Assessor (userType=3/1): valida VinculoAssessoria ativo.
///   - Corretor (userType=4): valida DelegacaoCarteira ativa delegada pelo assessor dono.
///   - Somente leitura (GET/HEAD/OPTIONS) — exceto geração de PDF.
///   - Sobrescreve EffectiveUserId com clienteId; RealUserId = quem está logado.
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
            var rawId    = context.User.FindFirstValue("nameid");

            if (userType is not ("3" or "1" or "4"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Acesso negado." });
                return;
            }

            if (!Guid.TryParse(rawId, out var userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // No view-as, assessor/corretor PODE alterar os dados do cliente (patrimônio,
            // ativos, dívidas, investimentos, projeção…). A ÚNICA exceção é a Gestão Pessoal
            // (lançamentos, categorias, metas), que permanece somente leitura.
            var ehGestaoPessoal =
                context.Request.Path.StartsWithSegments("/api/lancamentos", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/api/categorias",  StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/api/metas",       StringComparison.OrdinalIgnoreCase);

            if (ehGestaoPessoal &&
                !HttpMethods.IsGet(context.Request.Method) &&
                !HttpMethods.IsHead(context.Request.Method) &&
                !HttpMethods.IsOptions(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "A Gestão Pessoal é somente leitura na visão do cliente." });
                return;
            }

            bool autorizado;

            if (userType is "3" or "1")
            {
                // Assessor: valida VinculoAssessoria próprio
                autorizado = await db.VinculosAssessoria.AnyAsync(v =>
                    v.AssessorId == userId &&
                    v.ClienteId  == clienteId &&
                    v.AceitoEm   != null &&
                    v.RevogadoEm == null, context.RequestAborted);
            }
            else
            {
                // Corretor: valida DelegacaoCarteira ativa
                autorizado = await db.DelegacoesCarteira.AnyAsync(d =>
                    d.CorretorId  == userId &&
                    d.ClienteId   == clienteId &&
                    d.RevogadoEm  == null, context.RequestAborted);
            }

            if (!autorizado)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Vínculo de assessoria/delegação não encontrado ou revogado." });
                return;
            }

            context.Items["EffectiveUserId"] = clienteId;
            context.Items["RealUserId"]      = userId;
        }

        await next(context);
    }
}
