using Login.Domain.Common;

namespace Login.Domain.Entities;

public class CargoAgentClient : Entity
{
    public int CargoAgentCompanyId { get; private set; }
    public int CargoAgentClientCompanyId { get; private set; }
    public bool Associated { get; private set; }
    public string? UnassociatedReason { get; private set; }

    public CargoAgentPermission? Permissions { get; private set; }

    private CargoAgentClient() : base(Guid.Empty) { }

    public CargoAgentClient(Guid id, int cargoAgentCompanyId, int cargoAgentClientCompanyId) : base(id)
    {
        CargoAgentCompanyId = cargoAgentCompanyId;
        CargoAgentClientCompanyId = cargoAgentClientCompanyId;
        Associated = true;
    }

    public void UpdateAssociation(bool associated, string? unassociatedReason = null)
    {
        Associated = associated;
        UnassociatedReason = unassociatedReason;
        SetUpdated();
    }
}
