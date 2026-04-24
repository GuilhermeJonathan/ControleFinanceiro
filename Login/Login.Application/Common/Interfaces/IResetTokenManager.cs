using Login.Domain.Entities;

namespace Login.Application.Common.Interfaces;

public interface IResetTokenManager
{
    Task GenerateAndSendAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(Guid userId, string token, CancellationToken cancellationToken = default);
}
