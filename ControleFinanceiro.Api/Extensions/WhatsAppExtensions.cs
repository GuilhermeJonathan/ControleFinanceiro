using ControleFinanceiro.Api.Services;
using ControleFinanceiro.Api.WhatsApp;
using ControleFinanceiro.Application.Common.Interfaces;

namespace ControleFinanceiro.Api.Extensions;

public static class WhatsAppExtensions
{
    public static IServiceCollection AddWhatsApp(this IServiceCollection services)
    {
        services.AddHttpClient("whatsapp");
        services.AddHttpClient("openai");

        // IAiService — serviço de IA reutilizável em toda a aplicação
        services.AddScoped<IAiService, AzureOpenAiService>();

        services.AddScoped<WhatsAppSenderService>();
        services.AddScoped<WhatsAppMediaService>();
        return services;
    }
}
