namespace Login.Domain.Entities;

public class HierarchyCompany
{
    public Guid HierarchyId { get; private set; }
    public int ClientId { get; private set; }

    private HierarchyCompany() { }

    public HierarchyCompany(Guid hierarchyId, int clientId)
    {
        HierarchyId = hierarchyId;
        ClientId = clientId;
    }
}
