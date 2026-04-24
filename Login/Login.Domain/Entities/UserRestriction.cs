namespace Login.Domain.Entities;

public class UserRestriction
{
    public Guid UserId { get; private set; }
    public Guid ModuleId { get; private set; }
    public int CompanyId { get; private set; }

    private UserRestriction() { }

    public UserRestriction(Guid userId, Guid moduleId, int companyId)
    {
        UserId = userId;
        ModuleId = moduleId;
        CompanyId = companyId;
    }
}
