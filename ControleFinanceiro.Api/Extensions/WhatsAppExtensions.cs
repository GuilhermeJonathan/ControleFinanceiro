using ControleFinanceiro.Api.WhatsApp;

namespace ControleFinanceiro.Api.Extensions;

public static class WhatsAppExtensions
{
    public static IServiceCollection AddWhatsApp(this IServiceCollection services)
    {
        services.AddHttpClient("whatsapp");
        services.AddHttpClient("openai");
        services.AddScoped<WhatsAppSenderService>();
        services.AddScoped<WhatsAppMediaService>();
        return services;
    }
}
