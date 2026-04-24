namespace Login.Domain.Entities;

public class CargoAgentPermission
{
    public Guid CargoAgentId { get; private set; }
    public bool Documents { get; private set; }
    public bool Tracking { get; private set; }
    public bool Booking { get; private set; }
    public bool Bl { get; private set; }

    private CargoAgentPermission() { }

    public CargoAgentPermission(Guid cargoAgentId)
    {
        CargoAgentId = cargoAgentId;
    }

    public void Update(bool documents, bool tracking, bool booking, bool bl)
    {
        Documents = documents;
        Tracking = tracking;
        Booking = booking;
        Bl = bl;
    }
}
