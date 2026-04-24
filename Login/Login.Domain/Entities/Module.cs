using Login.Domain.Common;

namespace Login.Domain.Entities;

public class Module : Entity
{
    public string Name { get; private set; } = string.Empty;
    public bool HiddenMenu { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<ModuleFunction> Functions { get; private set; } = new List<ModuleFunction>();

    private Module() : base(Guid.Empty) { }

    public Module(Guid id, string name, bool hiddenMenu = false) : base(id)
    {
        Name = name;
        HiddenMenu = hiddenMenu;
        IsActive = true;
    }
}
