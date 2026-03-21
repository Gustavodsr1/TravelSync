using TravelSync.SharedKernel.Primitives;

namespace TravelSync.Trip.API.Domain;

public class TripMember : Entity
{
    public Guid TripId { get; private set; }
    public Guid UserId { get; private set; }
    public MemberRole Role { get; private set; }
    public MembershipStatus MembershipStatus { get; private set; }

    private TripMember() { }

    internal static TripMember CreateOwner(Guid tripId, Guid userId) =>
        new()
        {
            TripId = tripId,
            UserId = userId,
            Role = MemberRole.Owner,
            MembershipStatus = MembershipStatus.Accepted
        };

    public static TripMember CreateGuest(Guid tripId, Guid userId) =>
        new()
        {
            TripId = tripId,
            UserId = userId,
            Role = MemberRole.Guest,
            MembershipStatus = MembershipStatus.Pending
        };

    public void Accept() => MembershipStatus = MembershipStatus.Accepted;
    public void Reject() => MembershipStatus = MembershipStatus.Rejected;
}

public enum MemberRole
{
    Owner = 1,
    Guest = 2
}

public enum MembershipStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}
