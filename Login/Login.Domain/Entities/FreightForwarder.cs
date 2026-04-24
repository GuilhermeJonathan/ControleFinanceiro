using Login.Domain.Common;

namespace Login.Domain.Entities;

public class FreightForwarder : Entity
{
    public string CompanyName { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }

    public FreightForwarderPermission? Permissions { get; private set; }

    private FreightForwarder() : base(Guid.Empty) { }

    public FreightForwarder(Guid id, string companyName, string document, string? email = null) : base(id)
    {
        CompanyName = companyName;
        Document = document;
        Email = email;
        IsActive = true;
    }

    public void Update(string companyName, string? email)
    {
        CompanyName = companyName;
        Email = email;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
