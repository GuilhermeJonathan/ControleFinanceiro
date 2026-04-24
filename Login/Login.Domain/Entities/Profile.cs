using Login.Domain.Common;

namespace Login.Domain.Entities;

public class Profile : Entity
{
    public string Name { get; private set; } = string.Empty;
    public UserType UserTypeId { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    private Profile() : base(Guid.Empty) { }

    public Profile(Guid id, string name, UserType userTypeId) : base(id)
    {
        Name = name;
        UserTypeId = userTypeId;
        IsActive = true;
    }

    public void Update(string name, UserType userTypeId)
    {
        Name = name;
        UserTypeId = userTypeId;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
