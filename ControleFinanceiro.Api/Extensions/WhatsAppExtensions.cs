using ControleFinanceiro.Api.WhatsApp;

namespace ControleFinanceiro.Api.Extensions;

public static class WhatsAppExtensions
{
    public static IServiceCollection AddWhatsApp(this IServiceCollection services)
    {
        services.AddHttpClient("whatsapp");
        services.AddScoped<WhatsAppSenderService>();
        return services;
    }
}
