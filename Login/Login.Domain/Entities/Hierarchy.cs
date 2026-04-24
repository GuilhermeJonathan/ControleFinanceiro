using Login.Domain.Common;

namespace Login.Domain.Entities;

public class Hierarchy : Entity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public ICollection<HierarchyCompany> Companies { get; private set; } = new List<HierarchyCompany>();

    private Hierarchy() : base(Guid.Empty) { }

    public Hierarchy(Guid id, string name) : base(id)
    {
        Name = name;
        IsActive = true;
    }

    public void Update(string name)
    {
        Name = name;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
