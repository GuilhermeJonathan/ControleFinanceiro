namespace Login.Domain.Entities;

public class FreightForwarderPermission
{
    public Guid FreightForwarderId { get; private set; }
    public bool Documents { get; private set; }
    public bool Tracking { get; private set; }
    public bool Booking { get; private set; }
    public bool Bl { get; private set; }

    private FreightForwarderPermission() { }

    public FreightForwarderPermission(Guid freightForwarderId)
    {
        FreightForwarderId = freightForwarderId;
    }

    public void Update(bool documents, bool tracking, bool booking, bool bl)
    {
        Documents = documents;
        Tracking = tracking;
        Booking = booking;
        Bl = bl;
    }
}
