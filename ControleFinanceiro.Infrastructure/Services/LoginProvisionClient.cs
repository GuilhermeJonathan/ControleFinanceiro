using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ControleFinanceiro.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ControleFinanceiro.Infrastructure.Services;

public class LoginProvisionClient(HttpClient http, IConfiguration configuration) : ILoginProvisionClient
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<ProvisionContaResult> ProvisionAsync(
        string name, string email, string password, string? document, int userTypeId,
        CancellationToken ct = default)
    {
        var baseUrl = configuration["LoginApi:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("LoginApi:BaseUrl não configurado.");
        var serviceKey = configuration["ServiceAuth:ApiKey"]
            ?? throw new InvalidOperationException("ServiceAuth:ApiKey não configurado.");

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/user/provision");
        req.Headers.Add("X-Service-Key", serviceKey);
        req.Content = JsonContent.Create(new { name, email, password, document, userTypeId });

        using var resp = await http.SendAsync(req, ct);

        if (resp.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException(
                "Já existe uma conta com este e-mail. A senha informada não confere — use a senha da sua conta.");

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException("Não foi possível criar a conta. Tente novamente.");

        var dto = await resp.Content.ReadFromJsonAsync<ProvisionResponse>(JsonOpts, ct)
            ?? throw new InvalidOperationException("Resposta inválida do serviço de contas.");

        return new ProvisionContaResult(dto.AccessToken, dto.UserId, dto.Created);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var baseUrl = configuration["LoginApi:BaseUrl"]?.TrimEnd('/');
        var serviceKey = configuration["ServiceAuth:ApiKey"];
        // Sem config, não dá pra validar — falha aberta (deixa seguir) para não travar convites.
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(serviceKey))
            return false;

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/internal/email-exists/{Uri.EscapeDataString(email)}");
            req.Headers.Add("X-Service-Key", serviceKey);
            using var resp = await http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return false;
            var dto = await resp.Content.ReadFromJsonAsync<EmailExistsResponse>(JsonOpts, ct);
            return dto?.Exists ?? false;
        }
        catch
        {
            return false; // falha de rede → não bloqueia o envio
        }
    }

    private record ProvisionResponse(string AccessToken, Guid UserId, bool Created);
    private record EmailExistsResponse(bool Exists, int? UserTypeId);
}
