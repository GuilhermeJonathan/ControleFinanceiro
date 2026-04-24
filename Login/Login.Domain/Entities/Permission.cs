namespace Login.Domain.Entities;

public class Permission
{
    public Guid ProfileId { get; private set; }
    public Guid ModuleId { get; private set; }
    public Guid FunctionId { get; private set; }

    private Permission() { }

    public Permission(Guid profileId, Guid moduleId, Guid functionId)
    {
        ProfileId = profileId;
        ModuleId = moduleId;
        FunctionId = functionId;
    }
}
