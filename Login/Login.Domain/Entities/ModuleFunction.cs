using Login.Domain.Common;

namespace Login.Domain.Entities;

public class ModuleFunction : Entity
{
    public Guid ModuleId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private ModuleFunction() : base(Guid.Empty) { }

    public ModuleFunction(Guid id, Guid moduleId, string name) : base(id)
    {
        ModuleId = moduleId;
        Name = name;
    }
}
