using System.Security.Claims;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Api.Middleware;

public class FamiliaContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var rawId = context.User.FindFirstValue("nameid");
            if (Guid.TryParse(rawId, out var userId))
            {
                var donoId = await db.VinculosFamiliares
                    .Where(v => v.MembroId == userId && v.AceitoEm != null)
                    .Select(v => (Guid?)v.DonoId)
                    .FirstOrDefaultAsync();
                context.Items["EffectiveUserId"] = donoId ?? userId;
                context.Items["RealUserId"] = userId;
            }
        }
        await next(context);
    }
}
