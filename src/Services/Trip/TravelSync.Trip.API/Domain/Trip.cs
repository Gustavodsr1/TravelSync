using TravelSync.SharedKernel.Primitives;

namespace TravelSync.Trip.API.Domain;

public class Trip : Entity
{
    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public TripStatus Status { get; private set; } = TripStatus.Active;

    private readonly List<TripMember> _members = [];
    public IReadOnlyCollection<TripMember> Members => _members.AsReadOnly();

    private readonly List<TripDestination> _destinations = [];
    public IReadOnlyCollection<TripDestination> Destinations => _destinations.AsReadOnly();

    private Trip() { }

    public static Trip Create(Guid ownerUserId, string name, DateOnly startDate, DateOnly endDate)
    {
        var trip = new Trip
        {
            OwnerUserId = ownerUserId,
            Name = name,
            StartDate = startDate,
            EndDate = endDate
        };

        trip._members.Add(TripMember.CreateOwner(trip.Id, ownerUserId));

        return trip;
    }

    public void Update(string name, DateOnly startDate, DateOnly endDate)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Cancel() => Status = TripStatus.Cancelled;

    public void AddDestination(string country, string city, decimal? latitude, decimal? longitude, int visitOrder)
    {
        _destinations.Add(TripDestination.Create(Id, country, city, latitude, longitude, visitOrder));
    }

    public bool IsOwner(Guid userId) => OwnerUserId == userId;

    public bool IsMemberAccepted(Guid userId) =>
        _members.Any(m => m.UserId == userId && m.MembershipStatus == MembershipStatus.Accepted);

    public bool CanAccess(Guid userId) => IsOwner(userId) || IsMemberAccepted(userId);
}

public enum TripStatus
{
    Active = 1,
    Cancelled = 2
}
