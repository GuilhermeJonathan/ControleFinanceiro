using Login.Application.Common.Interfaces;
using Login.Domain.Entities;

namespace Login.Infrastructure.Services;

public class ResetTokenManager : IResetTokenManager
{
    private static readonly Dictionary<Guid, (string Token, DateTime Expires)> _tokens = new();

    public Task GenerateAndSendAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString("N");
        _tokens[user.Id] = (token, DateTime.UtcNow.AddHours(2));

        // Em produção: enviar por e-mail/SMS via INotificationService
        return Task.CompletedTask;
    }

    public Task<bool> ValidateAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        if (!_tokens.TryGetValue(userId, out var entry))
            return Task.FromResult(false);

        if (entry.Expires < DateTime.UtcNow)
        {
            _tokens.Remove(userId);
            return Task.FromResult(false);
        }

        var valid = entry.Token == token;
        if (valid) _tokens.Remove(userId);

        return Task.FromResult(valid);
    }
}
