namespace ControleFinanceiro.Application.Common.Interfaces;

public record ProvisionContaResult(string AccessToken, Guid UserId, bool Created);

/// <summary>
/// Chama a API de Login (server-to-server, com service key) para criar a conta do
/// convidado — ou autenticar se já existir — a partir de um convite já validado aqui.
/// </summary>
public interface ILoginProvisionClient
{
    Task<ProvisionContaResult> ProvisionAsync(
        string name, string email, string password, string? document, int userTypeId,
        CancellationToken ct = default);
}
