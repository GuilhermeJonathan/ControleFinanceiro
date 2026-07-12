namespace ControleFinanceiro.Application.Common.Interfaces;

public record UserContato(string? Nome, string? Email, string? AvatarUrl = null);

public interface IUserNameLookup
{
    Task<string?> GetNomeAsync(Guid userId, CancellationToken ct = default);
    Task<UserContato?> GetContatoAsync(Guid userId, CancellationToken ct = default);
}
