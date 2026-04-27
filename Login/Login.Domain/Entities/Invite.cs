using Login.Domain.Common;

namespace Login.Domain.Entities;

public class Invite : Entity
{
    public string Token { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private Invite() : base(Guid.Empty) { }

    public Invite(
        Guid id,
        string token,
        DateTime expiresAt,
        Guid createdByUserId,
        string? email = null)
        : base(id)
    {
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByUserId = createdByUserId;
        Email = email;
    }

    public bool IsValid => UsedAt == null && ExpiresAt > DateTime.UtcNow;

    public void Use(string usedByEmail)
    {
        UsedAt = DateTime.UtcNow;
        SetUpdated();
    }
}
