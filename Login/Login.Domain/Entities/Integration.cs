using Login.Domain.Common;

namespace Login.Domain.Entities;

public class Integration : Entity
{
    public string ClientId { get; private set; } = string.Empty;
    public string ClientSecretHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Integration() : base(Guid.Empty) { }

    public Integration(Guid id, string clientId, string clientSecretHash) : base(id)
    {
        ClientId = clientId;
        ClientSecretHash = clientSecretHash;
        IsActive = true;
    }
}
