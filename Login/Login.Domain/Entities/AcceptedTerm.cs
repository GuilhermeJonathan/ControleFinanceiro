using Login.Domain.Common;

namespace Login.Domain.Entities;

public class AcceptedTerm : Entity
{
    public Guid UserId { get; private set; }
    public string TermName { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private AcceptedTerm() : base(Guid.Empty) { }

    public AcceptedTerm(Guid id, Guid userId, string termName, string? ipAddress, string? userAgent) : base(id)
    {
        UserId = userId;
        TermName = termName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
